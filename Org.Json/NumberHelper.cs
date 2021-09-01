/*****************************************************************************
   Copyright 2021 The vladkryv@github.com. All Rights Reserved.
   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at
       http://www.apache.org/licenses/LICENSE-2.0
   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
******************************************************************************/
namespace Org.Json
{
    internal static class NumberHelper
    {
        public static bool IsNumber(object value)
        {
            return value is sbyte || value is byte || value is short
                   || value is ushort || value is int || value is uint
                   || value is long || value is ulong || value is float
                   || value is double || value is decimal;
        }
    }
}