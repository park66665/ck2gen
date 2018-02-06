using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrusaderKingsStoryGen.PropertyPageProxies
{
    class CultureProxy
    {
        private CultureParser culture;

        public CultureProxy(CultureParser title)
        {
            this.culture = title;
        }
        [Category("Culture Details"),
         DisplayName("Name")]
        public string Name
        {
            get
            {
                return culture.LanguageName;

            }
            set
            {
                culture.LanguageName = value;
                LanguageManager.instance.Add(culture.Scope.Name, value);

                Form1.instance.RefreshTree();
            }
        }

        [Category("Graphical Details"),
        DisplayName("Portrait")]
        public CultureParser.gfxStyles Portrait
        {
            get {
                    CultureParser.gfxStyles v = CultureParser.gfxStyles.africangfx;
                
                    Enum.TryParse<CultureParser.gfxStyles>(culture.dna.portraitPool[0], out v);
                    return v;
                }
            set
            {
                culture.dna.portraitPool.Clear();
                culture.dna.portraitPool.Add(value.ToString());
                culture.ScopeCultureDetails();
            }
        }

        [Category("Character Names"),
         DisplayName("Male Names")]
        [Editor(@"System.Windows.Forms.Design.StringCollectionEditor," +
            "System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
            typeof(System.Drawing.Design.UITypeEditor))]
        [TypeConverter(typeof(CsvConverter))]

        public List<string> MaleNames
        {
            get { return culture.dna.maleNameBlockSet; }
            
        }
        [Category("Character Names"),
         DisplayName("Female Names")]
        [Editor(@"System.Windows.Forms.Design.StringCollectionEditor," +
            "System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
            typeof(System.Drawing.Design.UITypeEditor))]
        [TypeConverter(typeof(CsvConverter))]
        public List<string> FemaleNames
        {
            get { return culture.dna.femaleNameBlockSet; }
           
        }

        [Category("Title Language"),
        DisplayName("Emperor Title")]
        public string EmperorTitle
        {
            get
            {
                return culture.dna.empTitle.Lang();

            }
            set
            {
                culture.dna.empTitle = value.AddSafe();

                Form1.instance.RefreshTree();
            }
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
                return Color.FromArgb(255, culture.r, culture.g, culture.b);

            }
            set
            {
                culture.r = value.R;
                culture.g = value.G;
                culture.b = value.B;
            }
        }

        [Category("Title Language"),
        DisplayName("King Title")]
        public string KingTitle
        {
            get
            {
                return culture.dna.kingTitle.Lang();

            }
            set
            {
                culture.dna.kingTitle = value.AddSafe();

                Form1.instance.RefreshTree();
            }
        }
        [Category("Title Language"),
             DisplayName("Duke Title")]
        public string DukeTitle
        {
            get
            {
                return culture.dna.dukeTitle.Lang();

            }
            set
            {
                culture.dna.dukeTitle = value.AddSafe();
                

                Form1.instance.RefreshTree();
            }
        }
        [Category("Title Language"),
                   DisplayName("Count Title")]
        public string CountTitle
        {
            get
            {
                return culture.dna.countTitle.Lang();

            }
            set
            {
                culture.dna.countTitle= value.AddSafe();
                Form1.instance.RefreshTree();
            }
        }
        [Category("Title Language"),
                      DisplayName("Baron Title")]
        public string BaronTitle
        {
            get
            {
                return culture.dna.baronTitle.Lang();

            }
            set
            {
                culture.dna.baronTitle= value.AddSafe();

                Form1.instance.RefreshTree();
            }
        }
        [Category("Title Language"),
                          DisplayName("Mayor Title")]
        public string MayorTitle
        {
            get
            {
                return culture.dna.mayorTitle.Lang();

            }
            set
            {
                culture.dna.mayorTitle = value.AddSafe();

                Form1.instance.RefreshTree();
            }
        }

    }
}
