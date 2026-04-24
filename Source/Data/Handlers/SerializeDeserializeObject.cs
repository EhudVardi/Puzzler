using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace Data
{
    public class SerializeDeserializeObject
    {
        XmlSerializer serializer;
        StreamWriter writer;
        StreamReader reader;

        public void SerializePuzzle(string xmlFilePath, object puzzle, Type type)
        {
            serializer = new XmlSerializer(type);
            using (writer = new StreamWriter(xmlFilePath))
                serializer.Serialize(writer.BaseStream, puzzle);
        }

        public object DeserializePuzzle(string xmlFilePath, Type type)
        {
            serializer = new XmlSerializer(type);
            using (reader = new StreamReader(xmlFilePath))
                return serializer.Deserialize(reader.BaseStream);
        }

    }

	internal class SerializeableObject<T> where T : class
    {
        internal string Serialize(T p_obj)
        {
            string xmlStr = null;
            XmlSerializer s = new XmlSerializer(typeof(T));
            TextWriter w = new StringWriter();
            s.Serialize(w, typeof(T));
            xmlStr = w.ToString();
            return xmlStr;
        }
        internal T Deserialize(string p_strXml)
        {
            T xmlobj;
            XmlSerializer s = new XmlSerializer(typeof(T));
            TextReader r = new StringReader(p_strXml);
            xmlobj = s.Deserialize(r) as T;
            return xmlobj;
        }
    }
    internal class SerializableLogMessage : SerializeableObject<SerializableLogMessage>
    {
        internal string p_category { get; set; }
        internal string p_additionalInfo { get; set; }
        internal object exType { get; set; }
        internal object exMessage { get; set; }
        internal object exHelpLink { get; set; }
        internal object exErrorCode { get; set; }
        internal object exNativeErrorCode { get; set; }
        internal object exStackTrace { get; set; }

        public SerializableLogMessage()
        {

        }
        public SerializableLogMessage(
                    string p_category,
                    string p_additionalInfo,
                    object exType,
                    object exMessage,
                    object exHelpLink,
                    object exErrorCode,
                    object exNativeErrorCode,
                    object exStackTrace)
        {
            this.p_category = p_category;
            this.p_additionalInfo = p_additionalInfo;
            this.exType = exType;
            this.exMessage = exMessage;
            this.exHelpLink = exHelpLink;
            this.exErrorCode = exErrorCode;
            this.exNativeErrorCode = exNativeErrorCode;
            this.exStackTrace = exStackTrace;
        }
    }
    
}
