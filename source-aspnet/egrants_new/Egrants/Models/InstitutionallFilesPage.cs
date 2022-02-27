using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace egrants_new.Egrants.Models
{
    public class InstitutionallFilesPage
    {

//        public string InstitutionName { get; set; } 
        public InstitutionalOrg SelectedInstitutionalOrg { get; set; }
        public List<InstitutionalOrg> OrgList { get; set; }
        public List<InstitutionalDocFiles> DocFiles { get; set; }
        public List<InstitutionalOrgCategory> OrgCategories { get; set; }
        public InstitutionalFilesPageAction Action { get; set; }
        public InsitutionalOrgNameIndex SelectedCharacterIndex { get; set; }
        public List<InsitutionalOrgNameIndex> CharacterIndices { get; set; }


    }
}