using System.Xml;

namespace CrusaderKingsStoryGen
{
    interface ISerializeXml
    {
        void SaveProject(XmlWriter writer);
        void LoadProject(XmlReader reader);
    }
}