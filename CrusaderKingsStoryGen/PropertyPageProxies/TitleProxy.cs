using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cyotek.Windows.Forms;

namespace CrusaderKingsStoryGen.PropertyPageProxies
{
    class TitleProxy
    {
        private TitleParser title;
        
      
        public TitleProxy(TitleParser title)
        {
            this.title = title;
        }
        [Category("Appearance"),
        DisplayName("Color")]
        [Editor(@"CrusaderKingsStoryGen.PropertyPageProxies.MyColorEditor",
            typeof(System.Drawing.Design.UITypeEditor)),
             TypeConverter(typeof(MyColorConverter))]
 
        public Color Color
        {
            get
            {
                return title.color;

            }
            set
            {
                title.color = value;
                title.color2 = value;
                title.SetProperty("color", title.color);
                title.SetProperty("color2", title.color);
                foreach (var provinceParser in title.GetAllProvinces())
                {
                    provinceParser.SetProperty("color", title.color);
                    provinceParser.SetProperty("color2", title.color);
                }
            }
        }

        [Category("Title Details"),
            DisplayName("Name")]

        public string Name
        {
            get
            {
                return title.LangName;

            }
            set
            {

                title.RenameSoft(value);
                Form1.instance.RefreshTree(title);
            }
        }
        [Category("Title Details"),
                DisplayName("Tag")]

        public string TitleName
        {
            get
            {
                return title.Name;

            }
           
        }
        [Category("Ruler Details"),
            DisplayName("Name")]
        public string LeaderName
        {
            get
            {
                return title.Holder.ChrName;

            }
            set { title.Holder.ChrName = value; }
        }
        [Category("Ruler Details"),
            DisplayName("Dynasty")]
        public string LeaderDynasty
        {
            get
            {
                return (title.Holder.Dynasty.NameScope as ScriptCommand).Value.ToString();

            }
            set { title.Holder.Dynasty.Name = value; }
        }
        [Category("Liege Details"),
              DisplayName("Liege")]
        public string Liege
        {
            get
            {
                if (title.Liege != null)
                {
                    return title.Liege.Name;
                }

                return "<Independent>";
            }
            
        }
    }
}
