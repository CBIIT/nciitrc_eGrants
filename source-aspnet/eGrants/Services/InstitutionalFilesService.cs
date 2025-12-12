using System.Data;

using BitMiracle.LibTiff.Classic;

using eGrants.DAL;
using eGrants.DTOs;
using eGrants.Models;
using eGrants.Repositories.Interfaces;
using eGrants.Services.Interfaces;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

using RtfPipe.Tokens;

using static NPOI.HSSF.Util.HSSFColor;

namespace eGrants.Services
{
    public class InstitutionalFilesService : IInstitutionalFilesService
    {
        private readonly AppDbContext _context;
        private readonly IInstitutionalFilesRepository _institutionalFilesRepository;

        public InstitutionalFilesService(IInstitutionalFilesRepository institutionalFilesRepository, AppDbContext context)
        {
            _institutionalFilesRepository = institutionalFilesRepository;
            _context = context;
        }

        public async Task<InstitutionalOrg> FindOrg(int orgId, string orgName = "")
        {
            return await _institutionalFilesRepository.FindOrg(orgId, orgName);
        }

        public async Task<List<InsitutionalOrgNameIndex>> LoadOrgNameCharacterIndices()
        {
            return await _institutionalFilesRepository.LoadOrgNameCharacterIndices();
        }

        public async Task<List<InstitutionalDocFiles>> LoadOrgDocList(int org_id)
        {
            return await _institutionalFilesRepository.LoadOrgDocList(org_id);
        }

        public async Task<List<InstitutionalOrgCategory>> LoadOrgCategory(bool activeOnly)
        {
            return await _institutionalFilesRepository.LoadOrgCategory(activeOnly);
        }

        public async Task<List<InstitutionalOrg>> LoadOrgList(int indexId)
        {
            return await _institutionalFilesRepository.LoadOrgList(indexId);
        }

        public async Task<string> UpdateDocument(int docId, int categoryId, string startDate, string endDate, string ic, string userId, string comments)
        {
            return await _institutionalFilesRepository.UpdateDocument(docId, categoryId, startDate, endDate, ic, userId, comments);
        }

        public async Task DisableDoc(int docId, string userId)
        {
            await _institutionalFilesRepository.DisableDoc(docId, userId);
        }

        public async Task<string> GetDocID(int orgId, int categoryId, string fileType, string startDate, string endDate, string ic, string userId, string comments)
        {
            return await _institutionalFilesRepository.GetDocID(orgId, categoryId, fileType, startDate, endDate, ic, userId, comments);
        }

        public async Task<List<InstitutionalOrg>> SearchOrgList(string search_str)
        {
            return await _institutionalFilesRepository.SearchOrgList(search_str);
        }
    }
}
