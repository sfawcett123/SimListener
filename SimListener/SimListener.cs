using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;

using System.Collections.Generic;
using System.Linq;
using System.Collections;
using Microsoft.VisualStudio.PlatformUI;

namespace SimListener
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct ResultStructure
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string sValue;
    };

    internal class SimvarRequest : ObservableObject
    {
        public DEFINITION eDef = DEFINITION.Dummy;
        public REQUEST eRequest = REQUEST.Dummy;

        public string sName { get; set; }
        public bool bIsString { get; set; }
        public double dValue
        {
            set { m_dValue = value; }
            get { return m_dValue; }
        }
        private double m_dValue = 0.0;
        public string sValue
        {
            set { m_sValue = value; }
            get { return m_sValue; }
        }
        private string m_sValue = null;

        public string sUnits { get; set; }

        public bool bPending = true;
        public bool bStillPending
        {
            set { m_bStillPending = value; }
            get { return m_bStillPending; }
        }
        private bool m_bStillPending = false;

    };
    public class SimulatorData : EventArgs
    {
        public DateTime TimeReached { get; set; }
        public List<Dictionary<string,string>> AircraftData { get; set; } = new List<Dictionary<string, string>>();
    }
}
