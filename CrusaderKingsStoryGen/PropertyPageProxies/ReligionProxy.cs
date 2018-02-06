using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrusaderKingsStoryGen.PropertyPageProxies
{
    class ReligionProxy
    {
        private ReligionParser religion;

        public ReligionProxy(ReligionParser title)
        {
            this.religion = title;
        }
        [Category("Religion Details"),
         DisplayName("Name")]
        public string Name
        {
            get
            {
                return religion.LanguageName;

            }
            set
            {
                religion.LanguageName = value;
                LanguageManager.instance.Add(religion.Scope.Name, value);

                Form1.instance.RefreshTree();
            }
        }

        void test()
        {
            
        }

        [Category("Religion Head"),
         DisplayName("Head Name")]
        public string PopeName
        {
            get { return religion.PopeName.Lang(); }
            set
            {
                religion.PopeName = value.AddSafe();
                religion.ScopeReligionDetails();
            }
        }
        [Category("Religion Head"),
              DisplayName("Has Head")]
        public bool HasHead
        {
            get { return religion.hasLeader; }
            set
            {
                religion.hasLeader = value;
                religion.ScopeReligionDetails();
            }
        }
        [Category("Religion Traditions"),
                    DisplayName("Heir Designation")]
        public bool HeirDes
        {
            get { return religion.has_heir_designation; }
            set
            {
                religion.has_heir_designation = value;
                religion.ScopeReligionDetails();
            }
        }
        [Category("Religion Traditions"),
                    DisplayName("Concubines")]
        public int Concubines
        {
            get { return religion.max_consorts; }
            set
            {
                religion.max_consorts = value;
                religion.max_wives = 1;
                religion.ScopeReligionDetails();
            }
        }
        [Category("Marriage Rules"),
           DisplayName("Max Wives")]
        public int Wives
        {
            get { return religion.max_wives; }
            set
            {
                religion.max_wives = value;
                religion.max_consorts = 0;
                religion.ScopeReligionDetails();
            }
        }
        [Category("Marriage Rules"),
               DisplayName("Sibling Marriage")]
        public bool SiblingMarriage
        {
            get { return religion.bs_marriage; }
            set
            {
                religion.bs_marriage = value;
                religion.ScopeReligionDetails();
            }
        }
        [Category("Marriage Rules"),
                DisplayName("Parent Child Marriage")]
        public bool ParentChildMarriage
        {
            get { return religion.pc_marriage; }
            set
            {
                religion.pc_marriage = value;
                religion.ScopeReligionDetails();
            }
        }
        [Category("Marriage Rules"),
                    DisplayName("Uncle-Niece/Aunty-Nephew Marriage")]
        public bool UncleMarriage
        {
            get { return !religion.psc_marriage; }
            set
            {
                religion.psc_marriage = !value;
                religion.ScopeReligionDetails();
            }
        }
        [Category("Marriage Rules"),
                            DisplayName("Cousin Marriage")]
        public bool CousinMarriage
        {
            get { return !religion.cousin_marriage; }
            set
            {
                religion.cousin_marriage = !value;
                religion.ScopeReligionDetails();
            }
        }
        [Category("Marriage Rules"),
        DisplayName("Matrilineal Marriage")]
        public bool Matrilineal
        {
            get { return !religion.matrilineal_marriages; }
            set
            {
                religion.matrilineal_marriages = !value;
                religion.ScopeReligionDetails();
            }
        }
        [Category("Marriage Rules"),
             DisplayName("Religious Intermarrying")]
        public bool Intermarry
        {
            get { return !religion.intermarry; }
            set
            {
                religion.intermarry = !value;
                religion.ScopeReligionDetails();
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
                return Color.FromArgb(255, religion.r, religion.g, religion.b);
                
            }
            set
            {
                religion.r = value.R;
                religion.g = value.G;
                religion.b = value.B;
            }
        }

    }
}
