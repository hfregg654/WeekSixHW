﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreProject.Helpers
{
    public enum UserLevel
    {
        /// <summary> 前台使用者f </summary>
        Normal = 0,

        /// <summary>
        /// 系統管理者
        /// </summary>
        Admin = 1,

        /// <summary>
        /// 員工
        /// </summary>
        Employee = 2,

        /// <summary>
        /// 主管
        /// </summary>
        Supervisor = 3

    }
}
