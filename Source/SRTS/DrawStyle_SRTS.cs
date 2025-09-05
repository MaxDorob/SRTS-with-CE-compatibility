using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace SRTS
{
    public abstract class DrawStyle_SRTS : DrawStyle
    {
        protected Thing Transporter => (Find.DesignatorManager.SelectedDesignator as SRTSDesignator)?.transporter;
        public abstract BombingType BombingType { get; }
        public abstract int BombCountPerPoint { get; } 
    }
}
