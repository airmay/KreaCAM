using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace ProcessingProgram.Objects
{
    /// <summary>
    /// Инструмент
    /// </summary>
    [XmlType(TypeName = "Инструмент")]
    public class Tool
    {
        [XmlElement(ElementName = "Номер")]
        public int No { get; set; }
        [XmlElement(ElementName = "Тип")]
        public string Type { get; set; }
        [XmlElement(ElementName = "Наименование")]
        public String Name { get; set; }
        [XmlElement(ElementName = "Порядковый_номер")]
        public int OrderNo { get; set; }
//        public ToolType Type;
//        public ToolGroup Group;
        [XmlElement(ElementName = "Позиция")]
        public int Position { get; set; }
        [XmlElement(ElementName = "Толщина")]
        public double? Thickness { get; set; }
        [XmlElement(ElementName = "Диаметр")]
        public double? Diameter { get; set; }
        [XmlElement(ElementName = "Подача")]
        public int WorkSpeed { get; set; }
        [XmlElement(ElementName = "Скорость_опускания")]
        public int DownSpeed { get; set; }
        [XmlElement(ElementName = "Частота")]
        public int Frequency { get; set; }

        private const string ToolsFileName = "tools.xml";  // TODO путь к файлу инструментов

        public static List<Tool> LoadTools()
        {
            //string toolsFullFileName = Path.Combine((new FileInfo(this.GetType().Assembly.FullName)).DirectoryName, ToolsFileName);  TODO путь к файлу инструментов
            try
            {
                using (var fileStream = new FileStream(ToolsFileName, FileMode.Open))
                {
                    var serializer = new XmlSerializer(typeof (List<Tool>));
                    return serializer.Deserialize(fileStream) as List<Tool>;
                }
            }
            catch (FileNotFoundException e)
            {
                MessageBox.Show(String.Format("Файл инструментов не найден: {0}\n{1}", ToolsFileName, e.Message), "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);                
            }
            catch (Exception e)
            {
                MessageBox.Show(String.Format("Ошибка при открытии файла инструментов: \n{0}", e.Message), "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return new List<Tool>();
        }

        public static void SaveTools(List<Tool> tools)
        {
            try
            {
                var serializer = new XmlSerializer(typeof (List<Tool>));
                TextWriter writer = new StreamWriter(ToolsFileName);    // new StreamWriter(csvFileName, true, Encoding.UTF8)) {
                serializer.Serialize(writer, tools);
                writer.Close();
                MessageBox.Show("Файл инструментов успешно сохранен"," Сообщение");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при записи файла инструментов: \n" + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}