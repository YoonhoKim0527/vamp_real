using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vampire
{
    [CreateAssetMenu(fileName = "AttendanceTable", menuName = "Attendance/Table")]
    public class AttendanceTableSO : ScriptableObject
    {
        public AttendanceDayData[] dayData = new AttendanceDayData[28];
    }
}
