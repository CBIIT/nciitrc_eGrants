#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  NodeMapping.cs
// Solution: egrants_new
// Project:  egrants_new
// Created: 2022-03-31
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

namespace egrants_new.Integration.Models
{
    /// <summary>
    /// The ws node mapping.
    /// </summary>
    public class WSNodeMapping
    {
        /// <summary>
        /// Gets or sets the ws node mapping_ id.
        /// </summary>
        public int WSNodeMapping_Id { get; set; }

        /// <summary>
        /// Gets or sets the node name.
        /// </summary>
        public string NodeName { get; set; }

        /// <summary>
        /// Gets or sets the data type.
        /// </summary>
        public string DataType { get; set; }

        // public string DestinationTable { get; set; }
        /// <summary>
        /// Gets or sets the destination field.
        /// </summary>
        public string DestinationField { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether transform data.
        /// </summary>
        public bool TransformData { get; set; }

        /// <summary>
        /// Gets or sets the transformation func.
        /// </summary>
        public string TransformationFunc { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is primary key.
        /// </summary>
        public bool IsPrimaryKey { get; set; }
    }
}