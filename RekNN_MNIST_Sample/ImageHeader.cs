// Copyright 2026 Fuuta System Service LLC.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RekNN_MNIST_Sample
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct ImageHeader
    {
        internal unsafe fixed byte ID[4];

        internal unsafe fixed byte DataNum[4];


        internal unsafe fixed byte NRows[4];

        internal unsafe fixed byte NCols[4];
    }
}
