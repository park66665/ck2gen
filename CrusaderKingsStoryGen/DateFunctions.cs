using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrusaderKingsStoryGen.Simulation;

namespace CrusaderKingsStoryGen
{
    public static class DateFunctions
    {
        public static int MakeDOBAtLeastAdult(int inDOB)
        {
            int age = SimulationManager.instance.Year + 1 - inDOB;

            if (age < 16)
                inDOB -= (16 - age);

            return inDOB;
        }

    }
}
