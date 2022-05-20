﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SayDB;

record DbContext(string RootDataFolderPath, ConcurrentDictionary<Type, DbCollection> Collections)
{
}