#region FileHeader
// /****************************** Module Header ******************************\
// Module Name:  SaveState.cs
// Solution: egrants_new
// Project:  egrants_new
// Created: 2023-04-20
// Contributors:
//      - Briggs, Robin (NIH/NCI) [C] - briggsr2
//      -
// Copyright (c) National Institute of Health
// 
// <Description of the file>
// 
// This source is subject to the NIH Softwre License.
// See https://ncihub.org/resources/899/download/Guidelines_for_Releasing_Research_Software_04062015.pdf
// All other rights reserved.
// 
// THE SOFTWARE IS PROVIDED "AS IS" AND ANY EXPRESS OR IMPLIED
// WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT ARE DISCLAIMED. IN NO EVENT SHALL THE NATIONAL
// CANCER INSTITUTE (THE PROVIDER), THE NATIONAL INSTITUTES OF HEALTH, THE
// U.S. GOVERNMENT OR THE INDIVIDUAL DEVELOPERS BE LIABLE FOR ANY DIRECT,
// INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
// HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN
// ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
// POSSIBILITY OF SUCH DAMAGE.
// \***************************************************************************/
#endregion

using egrants_new.Egrants.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

namespace egrants_new.Egrants.Functions
{
    public struct SaveState
    {
        public int CountingProperty;

        public int appl_id;

        public string appl_type_code;

        public string ful_grant_num;

        public string latest_full_grant_num;

#region OriginReference
        private static SaveState origin = new SaveState();
        public static ref readonly SaveState Origin => ref origin;
#endregion

    }

#region ReadonlyOnlySavState
    readonly public struct ReadonlyOnlySavState
    {
        public ReadonlyOnlySavState(int countProperty, int appl_id, string appl_type_code, string full_grant_num, string latest_full_grant_num)
        {
            this.CountProperty = countProperty;
            this.ApplId = appl_id;
            this.ApplTypeCode = appl_type_code;
            this.FulGrantNum = full_grant_num;
            this.LatestFullGrantNum = latest_full_grant_num;
        }

        public int CountProperty { get; }

        public int ApplId { get; }

        public string ApplTypeCode { get; }

        public string FulGrantNum { get; }

        public string LatestFullGrantNum { get; }

        private static readonly ReadonlyOnlySavState origin = new ReadonlyOnlySavState();
        public static ref readonly ReadonlyOnlySavState Origin => ref origin;
    }
#endregion
}