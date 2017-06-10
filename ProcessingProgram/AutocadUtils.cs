using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
//using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using ProcessingProgram.Constants;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using Color = Autodesk.AutoCAD.Colors.Color;
using Exception = Autodesk.AutoCAD.Runtime.Exception;
using TransactionManager = Autodesk.AutoCAD.DatabaseServices.TransactionManager;
using System.Threading;

namespace ProcessingProgram
{
    public static class AutocadExtension
    {
        /// <summary>
        /// Длина
        /// </summary>
        /// <param name="curve"></param>
        /// <returns></returns>
        public static double GetLength(this DBObject curve)
        {
            if (curve == null)
                return 0;
            if (curve is Line)
                return (curve as Line).Length;
            if (curve is Arc)
                return (curve as Arc).Length;
            if (curve is Circle)
                return (curve as Circle).Diameter * Math.PI;
            if (curve is Polyline)
                return (curve as Polyline).Length;
            if (curve is Polyline2d)
                return (curve as Polyline2d).Length;
            AutocadUtils.ShowError("Неподдерживаемый тип кривой");
            return 0;
        }
    }

    /// <summary>
    /// Методы для работы с Автокадом
    /// </summary>
    public static class AutocadUtils
    {
        //private Database Database = HostApplicationServices.WorkingDatabase;

        private static Document Document { get; set; }
        private static Database Database { get; set; }
        private static Editor Editor { get; set; }
        private static TransactionManager TransactionManager { get; set; }

        /// <summary>
        /// Выбор объекта пользователем
        /// </summary>
        public static event EventHandler<EventArgs<ObjectId>> Focused;

        /// <summary>
        /// Список выделенных объектов
        /// </summary>
        public static event EventHandler<EventArgs<List<ObjectId>>> Selected;

        static AutocadUtils()
        {
            SetActiveDocument(Application.DocumentManager.MdiActiveDocument);
            //Application.DocumentManager.DocumentActivated += (sender, args) => SetActiveDocument(args.Document);     //TODO блокировать действия при смене документа
            //Application.DocumentManager.DocumentLockModeChanged += DocumentManagerOnDocumentLockModeChanged;
        }

/*        private static void DocumentManagerOnDocumentLockModeChanged(object sender, DocumentLockModeChangedEventArgs documentLockModeChangedEventArgs)
        {
            if (documentLockModeChangedEventArgs.GlobalCommandName.ToUpper() == "QUIT")
            {
                documentLockModeChangedEventArgs.Veto();
                Application.DocumentManager.DocumentLockModeChanged -= DocumentManagerOnDocumentLockModeChanged;
                //Здесь делаем, что нам надо перед закрытием
                Close();
            }
        }*/

        public static void Close()
        {
            Document.CloseAndDiscard();
            Application.Quit();
        }

        public static void WriteMessage(string message)
        {
            Editor.WriteMessage(message + Environment.NewLine);            
        }

        public static void ShowError(string error)
        {
            WriteMessage(error);
            MessageBox.Show(error, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private static void SetActiveDocument(Document document)
        {
            if (Editor != null)
                Editor.SelectionAdded -= OnSelectionAddedEventHandler;

            Document = document;
            Editor = Document.Editor;
            Database = Document.Database;
            TransactionManager = Database.TransactionManager;

            Editor.SelectionAdded += OnSelectionAddedEventHandler;
            Database.PlineEllipse = true;
        }

        public static DialogResult ShowModalDialog(Form form)
        {
            return Application.ShowModalDialog(form);
        }

        #region Панель со вкладками

        private static PaletteSet _paletteSet;

        public static void AddPaletteSet(string name, Control control)
        {
            if (_paletteSet == null)
                _paletteSet = new PaletteSet("Технология")
                                  {
                                      Style = PaletteSetStyles.NameEditable | PaletteSetStyles.ShowPropertiesMenu | PaletteSetStyles.ShowAutoHideButton | PaletteSetStyles.ShowCloseButton,
                                      MinimumSize = new Size(300, 200),
                                      KeepFocus = true,
                                      Visible = true
                                  };
            _paletteSet.Add(name, control);
        }

        public static void ShowPaletteSet()
        {
            _paletteSet.Visible = true;
        }

        #endregion

        #region Работа с выделенными объектами

        private static ObjectId _selectedObjectId = ObjectId.Null;
        private static bool _isSelectionAdded;

        /// <summary>
        /// Обработчик события выделения пользователем объекта автокада
        /// </summary>
        /// <param name="o"></param>
        /// <param name="args"></param>
        private static void OnSelectionAddedEventHandler(object o, SelectionAddedEventArgs args)
        {
            if (args.AddedObjects.Count == 1 && args.AddedObjects[0].ObjectId != _selectedObjectId)
            {
                _selectedObjectId = args.AddedObjects[0].ObjectId;
                OnFocused(new EventArgs<ObjectId>(_selectedObjectId));
                _isSelectionAdded = true;
            }
            else
            {
                if (_isSelectionAdded)
                    OnSelected(new EventArgs<List<ObjectId>>(args.AddedObjects.GetObjectIds().ToList()));
                _isSelectionAdded = false;
            }
        }

        private static void OnFocused(EventArgs<ObjectId> e)
        {
            if (Focused != null)
                Focused(null, e);
        }

        private static void OnSelected(EventArgs<List<ObjectId>> e)
        {
            if (Selected != null)
                Selected(null, e);
        }

        public static void SelectObject(ObjectId objectId)
        {
            SelectObjects(objectId, ObjectId.Null);
        }

        /// <summary>
        /// Выделение объекта автокада программное
        /// </summary>
        /// <param name="objectId"></param>
        public static void SelectObjects(ObjectId objectId, ObjectId toolObjectId)
        {
            var idList = new List<ObjectId> { objectId, toolObjectId };
            Editor.SetImpliedSelection(idList.Where(p => p != ObjectId.Null).ToArray());
            Editor.UpdateScreen();
        }

        /// <summary>
        /// Список выделенных объектов автокада
        /// </summary>
        /// <returns></returns>
        public static List<DBObject> GetSelectedObjects()
        {
            List<DBObject> result = null;

            using (var trans = TransactionManager.StartTransaction())
            {
                //var sel = Editor.SelectPrevious();  // вызов команды из командной строки
                //if (sel.Status != PromptStatus.OK)
                var sel = Editor.SelectImplied();   // вызов команды нажатием кнопки на тулбаре
                if (sel.Status == PromptStatus.OK)
                    result = sel.Value.GetObjectIds().Select(p => trans.GetObject(p, OpenMode.ForRead)).ToList();
                else
                    ShowError("Нет выбранных объектов");
                trans.Commit();
            }
            return result;
        }

        #endregion

        /// <summary>
        /// Получить стиль элемента обработки для действия
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="processAction">Действие</param>
        /// <returns></returns>
        private static ObjectId GetProcessLayer(Transaction trans, ActionType processAction)
        {
            ObjectId layerId;
            string layerName = processAction.ToString();
            LayerTable layerTbl = trans.GetObject(Database.LayerTableId, OpenMode.ForRead) as LayerTable;

            if (layerTbl.Has(layerName)) // Проверяем нет ли еще слоя с таким именем в чертеже
            {
                layerId = layerTbl[layerName];
            }
            else // создание нового слоя
            {
                layerTbl.UpgradeOpen();
                LayerTableRecord layer = new LayerTableRecord();
                layer.Name = layerName;
                switch (processAction)
                {
                    case ActionType.Move:
                    case ActionType.InitialMove:
                        layer.Color = Color.FromColor(System.Drawing.Color.Crimson);
                        break;
                    case ActionType.Cutting:
                    case ActionType.Penetration:
                        layer.Color = Color.FromColor(System.Drawing.Color.Green);
                        break;
                    case ActionType.Direction:
                        layer.Color = Color.FromColor(System.Drawing.Color.SpringGreen);
                        break;
                    case ActionType.EngageMove:
                    case ActionType.AapproachMove:
                        layer.Color = Color.FromColor(System.Drawing.Color.Blue);
                        break;
                    case ActionType.RetractMove:
                    case ActionType.DepartureMove:
                        layer.Color = Color.FromColor(System.Drawing.Color.White);
                        break;
                }

                LinetypeTable lineTypeTbl = ((LinetypeTable) (trans.GetObject(Database.LinetypeTableId, OpenMode.ForWrite)));
                ObjectId lineTypeID;
                string ltypeName = "Continuous";
                if (lineTypeTbl.Has(ltypeName))
                {
                    lineTypeID = lineTypeTbl[ltypeName];
                }
                else // создания стиля линий
                {
                    LinetypeTableRecord lineType = new LinetypeTableRecord();
                    lineType.Name = ltypeName;
                    lineTypeTbl.Add(lineType);
                    trans.AddNewlyCreatedDBObject(lineType, true);
                    lineTypeID = lineType.ObjectId;
                }
                layer.LinetypeObjectId = lineTypeID;
                layer.IsPlottable = true;
                layerId = layerTbl.Add(layer);
                trans.AddNewlyCreatedDBObject(layer, true);
            }
            return layerId;
        }

        /// <summary>
        /// Добавить кривую
        /// </summary>
        /// <param name="curve">Кривая</param>
        /// <param name="processAction">Действие</param>
        /// <returns>ИД кривой</returns>
        public static ObjectId AddCurveTransaction(Curve curve, ActionType processAction)
        {
            ObjectId objectId;
            using (Document.LockDocument())
            {
                using (var trans = TransactionManager.StartTransaction())
                {
                    BlockTable blockTable = trans.GetObject(Database.BlockTableId, OpenMode.ForRead, false) as BlockTable;
                    BlockTableRecord blockTableRecord = trans.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite, false) as BlockTableRecord;

                    curve.LayerId = GetProcessLayer(trans, processAction);
                    objectId = blockTableRecord.AppendEntity(curve);
                    trans.AddNewlyCreatedDBObject(curve, true);

                    trans.Commit();
                    //Editor.UpdateScreen();
                }
            }
            return objectId;
        }

        private static DocumentLock _documentLock;
        private static Transaction _transaction;

        public static ObjectId AddCurve(Curve curve, ActionType processAction)
        {
            ObjectId objectId;

            BlockTable blockTable = _transaction.GetObject(Database.BlockTableId, OpenMode.ForRead, false) as BlockTable;
            BlockTableRecord blockTableRecord = _transaction.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite, false) as BlockTableRecord;

            curve.LayerId = GetProcessLayer(_transaction, processAction);
            objectId = blockTableRecord.AppendEntity(curve);
            _transaction.AddNewlyCreatedDBObject(curve, true);

            return objectId;
        }

        public static void StartTransaction()
        {
            _documentLock = Document.LockDocument();
            _transaction = TransactionManager.StartTransaction();
        }

        public static void CommitTransaction()
        {
            _transaction.Commit();
            Editor.UpdateScreen();
        }

        public static void DisposeTransaction()
        {
            _transaction.Dispose();
            _documentLock.Dispose();
        }

        /// <summary>
        /// Удаление кривых
        /// </summary>
        /// <param name="objects">Список ИД кривых</param>
        public static void DeleteCurves(List<ObjectId> objects)
        {
            using (Document.LockDocument())
            {
                using (var trans = TransactionManager.StartTransaction())
                {
                    objects.FindAll(p => p != ObjectId.Null).ForEach(p => trans.GetObject(p, OpenMode.ForWrite).Erase());
                    trans.Commit();
                    Editor.UpdateScreen();
                }
            }
        }

        /// <summary>
        /// Получить кривую по ИД
        /// </summary>
        /// <param name="objectId">ИД кривой</param>
        /// <returns>Кривая</returns>
        public static Curve GetCurve(ObjectId objectId)
        {
            Curve curve;
            using (var trans = TransactionManager.StartTransaction())
            {
                curve = trans.GetObject(objectId, OpenMode.ForRead) as Curve;
                trans.Commit();
            }
            return curve;
        }

        /// <summary>
        /// Получить копию кривой со смещением в плоскости XY
        /// </summary>
        /// <param name="curve">Копируемая кривая</param>
        /// <param name="offset">Смещение</param>
        /// <returns>Созданная копия</returns>
        public static Curve GetOffsetCopy(Curve curve, double offset)
        {
            var sign = (curve is Line) ? 1 : -1; 
            return curve.GetOffsetCurves(offset * sign)[0] as Curve;
        }

        /// <summary>
        /// Получить копию кривой со смещением по Z
        /// </summary>
        /// <param name="curve">Копируемая кривая</param>
        /// <param name="displacement">Смещение</param>
        /// <returns>Созданная копия</returns>
        public static Curve GetDisplacementCopy(Curve curve, double displacement)
        {
            return curve.GetTransformedCopy(Matrix3d.Displacement(new Vector3d(0, 0, displacement))) as Curve;
        }

        /// <summary>
        /// Получить вектор касательной к кривой в точке
        /// </summary>
        /// <param name="objectId">ИД кривой</param>
        /// <param name="point">Точка</param>
        /// <returns>Вектор касательной</returns>
        public static Vector3d GetFirstDerivative(ObjectId objectId, Point3d point)
        {
            Vector3d vector;
            using (var trans = TransactionManager.StartTransaction())
            {
                var curve = trans.GetObject(objectId, OpenMode.ForRead) as Curve;
                vector = curve.GetFirstDerivative(point);
                trans.Commit();
            }
            return vector;
        }

        /// <summary>
        /// Список точек полилинии
        /// </summary>
        /// <param name="objectId"></param>
        /// <returns></returns>
        public static List<Point3d> GetPoints(ObjectId objectId)
        {
            var points = new List<Point3d>();
            using (var trans = TransactionManager.StartTransaction())
            {
                var curve = trans.GetObject(objectId, OpenMode.ForRead);
                if (curve is Polyline2d)
                    points.AddRange(from ObjectId acObjIdVert in (Polyline2d)curve
                                    select trans.GetObject(acObjIdVert, OpenMode.ForRead) as Vertex2d
                                    into vertex select vertex.Position);
                else if (curve is Polyline)
                    for (var i = 0; i < ((Polyline)curve).NumberOfVertices; i++)
                        points.Add(((Polyline)curve).GetPoint3dAt(i));
                
                trans.Commit();
            }
            return points;            
        }

        /// <summary>
        /// Разбить кривую на список кривых
        /// </summary>
        /// <param name="curve">Кривая</param>
        /// <param name="reverse">В обратном порядке</param>
        /// <returns>Список кривых</returns>
        public static List<Curve> Explode(Curve curve, bool reverse = false)
        {
            List<Curve> toolpathCurves;
            if (curve is Circle)
            {
                var circle = curve as Circle;
                toolpathCurves = new List<Curve> 
                { 
                    new Arc(circle.Center, circle.Radius, 0, Math.PI),
                    new Arc(circle.Center, circle.Radius, Math.PI, 0)
                };
            }
            else
            {
                var dbObjects = new DBObjectCollection();
                curve.Explode(dbObjects);
                toolpathCurves = dbObjects.Cast<Curve>().ToList();
            }
            if (reverse)
                toolpathCurves.Reverse();
            return toolpathCurves;
        }

        /// <summary>
        /// Разбить окружность на дуги
        /// </summary>
        /// <param name="curve">Окружность</param>
        /// <param name="reverse">В обратном порядке</param>
        /// <returns>Список дуг</returns>
        public static List<Curve> ExplodeCircle(Curve curve, bool reverse = false)
        {
            var circle = curve as Circle;
            var arc1 = new Arc(circle.Center, circle.Radius, 0, Math.PI);
            var arc2 = new Arc(circle.Center, circle.Radius, Math.PI, 0);
            return reverse ? new List<Curve> { arc1, arc2 } : new List<Curve> { arc2, arc1 };
        }

        /// <summary>
        /// Получить все кривые документа
        /// </summary>
        /// <returns></returns>
        public static List<DBObject> GetAllCurves()
        {
            List<DBObject> list = null;
            using (var trans = TransactionManager.StartTransaction())
            {
                PromptSelectionResult res = Editor.SelectAll();
                if (res.Status == PromptStatus.OK)
                    list = res.Value.Cast<SelectedObject>().Select(p => trans.GetObject(p.ObjectId, OpenMode.ForRead)).ToList();
                trans.Commit();
            }
            return list;
        }

        [Conditional("DEBUG")]
        public static void CreateTest()
        {

            using (Document.LockDocument())
            {
                using (var trans = TransactionManager.StartTransaction())
                {
                    BlockTable blockTable = trans.GetObject(Database.BlockTableId, OpenMode.ForRead, false) as BlockTable;
                    BlockTableRecord blockTableRecord = trans.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite, false) as BlockTableRecord;

                    List<Curve> curves;

                    if (false)
                    {
                        var arc = new Arc(new Point3d(2391.1289, 1383.962, 0), 563.3755, 12 * Math.PI / 180, 159 * Math.PI / 180);
                        curves = new List<Curve>
                        {
                                new Line(arc.EndPoint, new Point3d(2017, 1047, 0)),
                                new Line(new Point3d(2017, 1047, 0), new Point3d(2627, 1009, 0)),
                                new Line(arc.StartPoint, new Point3d(2627, 1009, 0)),
                                arc
                            };
                    }
                    else
                    {
                        Polyline acPoly = new Polyline();
                        acPoly.AddVertexAt(0, new Point2d(2000, 1500), -0.5, 0, 0);
                        acPoly.AddVertexAt(1, new Point2d(3000, 1500), 0, 0, 0);
                        acPoly.AddVertexAt(2, new Point2d(2800, 700), 0, 0, 0);
                        acPoly.AddVertexAt(3, new Point2d(2300, 700), 0, 0, 0);
                        acPoly.Closed = true;
                        curves = new List<Curve> { acPoly, new Line(new Point3d(-20, 40, 0), new Point3d(0, 0, 0)) };
                    }

                    foreach (Curve curve in curves)
                    {
                        blockTableRecord.AppendEntity(curve);
                        trans.AddNewlyCreatedDBObject(curve, true);
                    }
                    trans.Commit();
                    Editor.UpdateScreen();
                }
            }
        }

        // TODO - показ временной графики с помощью AutoCAD API http://adn-cis.org/ispolzovanie-tranzitnoj-grafiki.html

/*        public static void SetTransient(ObjectId objectId)
        {
            if (objectId == ObjectId.Null)
                return;
            using (var trans = TransactionManager.StartTransaction())
            {
                var curve = trans.GetObject(objectId, OpenMode.ForRead) as Curve;

                Circle marker = new Circle(curve.StartPoint, Vector3d.ZAxis, 100);
                marker.Color = Color.FromRgb(0, 255, 0);
                //_markers.Add(marker);

                IntegerCollection intCol = new IntegerCollection();
                TransientManager tm = TransientManager.CurrentTransientManager;

                tm.AddTransient
                    (
                        marker,
                        TransientDrawingMode.Highlight,
                        128,
                        intCol
                    );
                trans.Commit();
            }
        }*/

        public static void Test()
        {
            var curves = GetSelectedObjects();
            if (curves == null)
                return;
            var curve = curves.FirstOrDefault() as Curve;
            if (curve == null)
                return;

            var copy = curve.GetOffsetCurves(100)[0];
            AddCurve(copy as Curve, ActionType.Direction);
/*
            TypedValue[] filter = {new TypedValue((int) DxfCode.Start, "LWPOLYLINE")};

            SelectionFilter filterSset = new SelectionFilter(filter);

            PromptSelectionOptions pso = new PromptSelectionOptions();

            pso.SingleOnly = true; //comment this if you want to label multiple polylines

            pso.MessageForAdding = "Select a single polyline >> ";

            PromptSelectionResult psr = Editor.GetSelection(pso); //, filterSset);

            if (psr.Status != PromptStatus.OK)
                return;

            using (var trans = TransactionManager.StartTransaction())
            {
                var curve = trans.GetObject(psr.Value.GetObjectIds().First(), OpenMode.ForRead) as Curve;

                CreateFirstDerivative(curve, 0);
                CreateFirstDerivative(curve, curve.GetLength()/2);
                CreateFirstDerivative(curve, curve.GetLength());

                trans.Commit();
            }
*/
        }

        public static void CreateBlock()
        {
            string strBlockName = "Присоска квадратная";

            using (Document.LockDocument())
            {
                using (var trans = TransactionManager.StartTransaction())
                {
                    BlockTable blockTable = trans.GetObject(Database.BlockTableId, OpenMode.ForRead, false) as BlockTable;
                    //BlockTableRecord blockTableRecord = trans.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite, false) as BlockTableRecord;

                    try
                    {
                        // проверка на корректность символов в имени блока
                        SymbolUtilityServices.ValidateSymbolName(strBlockName, false);
                    }
                    catch
                    {
                        WriteMessage("nInvalid block name.");
                        return;
                    }
                    BlockTableRecord btrRecord;
                    ObjectId btrId;
                    if (!blockTable.Has(strBlockName))
                    {
                        // создаем ОПРЕДЕЛЕНИЕ блока  
                        // создаем примитивы, которые будет содержать блок
                        Polyline acPoly = new Polyline();
                        acPoly.AddVertexAt(0, new Point2d(0, 0), 0, 0, 0);
                        acPoly.AddVertexAt(1, new Point2d(100, 0), 0, 0, 0);
                        acPoly.AddVertexAt(2, new Point2d(100, 100), 0, 0, 0);
                        acPoly.AddVertexAt(3, new Point2d(0, 100), 0, 0, 0);
                        acPoly.Closed = true;
                        acPoly.ColorIndex = 3;

                        // создаем определение аттрибута
                        AttributeDefinition adAttr = new AttributeDefinition();
/*                        adAttr.Position = new Point3d(0, 0, 0);
                        adAttr.Tag = "ATTRDEF";
                        adAttr.TextString = "Присоска";*/

                        // создаем новое определение блока
                        btrRecord = new BlockTableRecord();
                        btrRecord.Name = strBlockName;
                        btrRecord.Comments = "Присоска для закрепления детали на станке";
                        blockTable.UpgradeOpen();

                        // добавляем его в таблицу блоков
                        btrId = blockTable.Add(btrRecord);
                        trans.AddNewlyCreatedDBObject(btrRecord, true);

                        // добавляем в определение блока примитивы
                        btrRecord.AppendEntity(acPoly);
                        trans.AddNewlyCreatedDBObject(acPoly, true);

                        // и аттрибут
//                        btrRecord.AppendEntity(adAttr);
//                        trans.AddNewlyCreatedDBObject(adAttr, true);
                    }
                    else
                    {
                        btrId = blockTable[strBlockName];
                        btrRecord = (BlockTableRecord)trans.GetObject(blockTable[strBlockName], OpenMode.ForWrite);
                    }
                    // теперь создадим экземпляр блока на чертеже
                    // получаем пространство модели
                    BlockTableRecord btrModelSpace = (BlockTableRecord) trans.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                    // создаем новый экземпляр блока на основе его определения
                    BlockReference brRefBlock = new BlockReference(new Point3d(2000, 1500, 0), btrId);

                    // добавляем экземпляр блока в базу данных пространства модели
                    btrModelSpace.AppendEntity(brRefBlock);
                    trans.AddNewlyCreatedDBObject(brRefBlock, true);

                    // задаем значение аттрибуета                   
//                    AttributeReference arAttr = new AttributeReference();
//                    arAttr.SetAttributeFromBlock(adAttr, brRefBlock.BlockTransform);
//                    arAttr.TextString = "Атрибут!";
//                    brRefBlock.AttributeCollection.AppendAttribute(arAttr);
//                    trans.AddNewlyCreatedDBObject(arAttr, true);

                    // закрываем транзакцию
                    trans.Commit();
                }
            }
        }

        // TODO сохранение данных в чертеже

        public static void SetExtDictionaryValueString(ObjectId ename, string key, string value)
       {
           if (ename == ObjectId.Null) throw new ArgumentNullException("ename");
           if (String.IsNullOrEmpty(key)) throw new ArgumentNullException("key");

           var doc = Application.DocumentManager.MdiActiveDocument;

           using (var transaction = doc.Database.TransactionManager.StartTransaction())
           {
               var entity = transaction.GetObject(ename, OpenMode.ForWrite);
               if (entity == null)
                   throw new DataException("Ошибка при записи текстового значения в ExtensionDictionary: entity с ObjectId=" + ename + " не найдена");

               //Получение или создание словаря extDictionary

               var extensionDictionaryId = entity.ExtensionDictionary;
               if (extensionDictionaryId == ObjectId.Null)
               {
                   entity.CreateExtensionDictionary();
                   extensionDictionaryId = entity.ExtensionDictionary;
               }
               var extDictionary = (DBDictionary)transaction.GetObject(extensionDictionaryId, OpenMode.ForWrite);


               // Запись значения в словарь
               if (String.IsNullOrEmpty(value))
               {
                   if (extDictionary.Contains(key))
                       extDictionary.Remove(key);
                   return;
               }
               var xrec = new Xrecord();
               xrec.Data = new ResultBuffer(new TypedValue((int)DxfCode.ExtendedDataAsciiString, value));
               extDictionary.SetAt(key, xrec);
               transaction.AddNewlyCreatedDBObject(xrec, true);
               Debug.WriteLine(entity.Handle + "['" + key + "'] = '" + value + "'");
               transaction.Commit();

           }
       }

        public static string GetExtDictionaryValueString(ObjectId ename, string key)
       {
           if (ename == ObjectId.Null) throw new ArgumentNullException("ename");
           if (String.IsNullOrEmpty(key)) throw new ArgumentNullException("key");


           var doc = Application.DocumentManager.MdiActiveDocument;

           using (var transaction = doc.Database.TransactionManager.StartTransaction())
           {
               var entity = transaction.GetObject(ename, OpenMode.ForRead);
               if (entity == null)
                   throw new DataException("Ошибка при чтении текстового значения из ExtensionDictionary: полилиния с ObjectId=" + ename + " не найдена");

               var extDictionaryId = entity.ExtensionDictionary;
               if (extDictionaryId == ObjectId.Null)
                   throw new DataException("Ошибка при чтении текстового значения из ExtensionDictionary: словарь не найден");
               var extDic = (DBDictionary)transaction.GetObject(extDictionaryId, OpenMode.ForRead);

               if (!extDic.Contains(key))
                   return null;
               var myDataId = extDic.GetAt(key);
               var readBack = (Xrecord)transaction.GetObject(myDataId, OpenMode.ForRead);
               return (string)readBack.Data.AsArray()[0].Value;
           }
       }

        public static ObjectId CreateToolCurve(double x, double y, double? height)
        {
            Polyline pline = new Polyline();
            pline.AddVertexAt(0, new Point2d(x, y), 0, 0, 0);
            pline.AddVertexAt(0, new Point2d(x, y - height.GetValueOrDefault()), 0.5, 0, 0);
            pline.AddVertexAt(0, new Point2d(50 + x, y - height.GetValueOrDefault()), 0, 0, 0);
            pline.AddVertexAt(0, new Point2d(50 + x, y), 0.5, 0, 0);
            pline.Closed = true;

            return AutocadUtils.AddCurve(pline, ActionType.Direction);
        }

//  Работа с глобальным словарем почти такая же, только объект DBDictionary получается так:

//  var dictionary = (DBDictionary) transaction.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForWrite);

    }
}
