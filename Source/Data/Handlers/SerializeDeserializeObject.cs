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
            try
            {
                serializer = new XmlSerializer(type);
                writer = new StreamWriter(xmlFilePath);
                serializer.Serialize(writer.BaseStream, puzzle);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                writer.Close();
                writer.Dispose();
            }
        }

        public object DeserializePuzzle(string xmlFilePath, Type type)
        {
            object puzzle = new object();
            
            try
            {
                serializer = new XmlSerializer(type);
                reader = new StreamReader(xmlFilePath);
                object deserialized = serializer.Deserialize(reader.BaseStream);

                puzzle = deserialized;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                reader.Close();
                reader.Dispose();
            }

            return puzzle;
        }

    }

    
}
