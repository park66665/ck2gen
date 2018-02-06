using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrusaderKingsStoryGen
{
    public class GenerationOptions
    {
        public static int IdealIndependentEmpireCount = 2;
        public static int IdealIndependentKingCount = 50;
        public static int IdealIndependentDukeCount = 12;
        public static int MaxConcurrentConquerers = 2;

        #region

        public static int KingdomStability
        {
            get { return _kingdomStability; }
            set
            {
                _kingdomStability = value;
                switch (_kingdomStability)
                {
                    case 0:
                        IdealIndependentKingCount = 25;
                        break;
                    case 1:
                        IdealIndependentKingCount = 35;
                        break;
                    case 2:
                        IdealIndependentKingCount = 50;
                        break;
                    case 3:
                        IdealIndependentKingCount = 60;
                        break;
                    case 4:
                        IdealIndependentKingCount = 70;
                        break;
                    case 5:
                        IdealIndependentKingCount = 80;
                        break;
                }
            }
        }

        #endregion
        #region EmpireStability
        public static int EmpireStability
        
        {
            get { return _empireStability; }
            set
            {
                _empireStability = value;
                switch (_empireStability)
                {
                    case 0:
                        IdealIndependentEmpireCount = 3;
                        break;
                    case 1:
                        IdealIndependentEmpireCount = 2;
                        break;
                    case 2:
                        IdealIndependentEmpireCount = 1;
                        break;
                    case 3:
                        IdealIndependentEmpireCount = 1;
                        break;
                    case 4:
                        IdealIndependentEmpireCount = 0;
                        break;
                    case 5:
                        IdealIndependentEmpireCount = 0;
                        break;
                }
            }
        }
        #endregion
        public static int GovernmentMutate { get; set; } = 2;
        public static int ReligionMutate { get; set; } = 2;
        public static int CultureMutate { get; set; } = 2;
        public static int TechAdvanceRate { get; set; } = 2;
        public static int TechSpreadRate { get; set; } = 2;
        public static int HoldingDevSpeed { get; set; }= 2;

 #region Conquerers
        private static int _conquerers = 2;
        private static int _empireStability = 2;
        private static int _kingdomStability = 2;

        public static int Conquerers
        {
            get { return _conquerers; }
            set
            {
                _conquerers = value;
                switch (value)
                {
                    case 0:
                        MaxConcurrentConquerers = 7;
                        break;
                    case 1:
                        MaxConcurrentConquerers = 5;
                        break;
                    case 2:
                        MaxConcurrentConquerers = 3;
                        break;
                    case 3:
                        MaxConcurrentConquerers = 2;
                        break;
                    case 4:
                        MaxConcurrentConquerers = 1;
                        break;
                    case 5:
                        MaxConcurrentConquerers = 0;
                        break;
                }
            }
        }
#endregion
    }
}
