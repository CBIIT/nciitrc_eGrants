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
using Serilog;

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
            try
            {
                var result = await _institutionalFilesRepository.FindOrg(orgId, orgName);
                Log.Information("Successfully executed FindOrg with orgId={OrgId}, orgName={OrgName}", orgId, orgName);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in FindOrg with orgId={OrgId}, orgName={OrgName}", orgId, orgName);
                throw;
            }
        }

        public async Task<List<InsitutionalOrgNameIndex>> LoadOrgNameCharacterIndices()
        {
            try
            {
                var result = await _institutionalFilesRepository.LoadOrgNameCharacterIndices();
                Log.Information("Successfully executed LoadOrgNameCharacterIndices");
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in LoadOrgNameCharacterIndices");
                throw;
            }
        }

        public async Task<List<InstitutionalDocFiles>> LoadOrgDocList(int org_id)
        {
            try
            {
                var result = await _institutionalFilesRepository.LoadOrgDocList(org_id);
                Log.Information("Successfully executed LoadOrgDocList with org_id={OrgId}", org_id);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in LoadOrgDocList with org_id={OrgId}", org_id);
                throw;
            }
        }

        public async Task<List<InstitutionalOrgCategory>> LoadOrgCategory(bool activeOnly)
        {
            try
            {
                var result = await _institutionalFilesRepository.LoadOrgCategory(activeOnly);
                Log.Information("Successfully executed LoadOrgCategory with activeOnly={ActiveOnly}", activeOnly);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in LoadOrgCategory with activeOnly={ActiveOnly}", activeOnly);
                throw;
            }
        }

        public async Task<List<InstitutionalOrg>> LoadOrgList(int indexId)
        {
            try
            {
                var result = await _institutionalFilesRepository.LoadOrgList(indexId);
                Log.Information("Successfully executed LoadOrgList with indexId={IndexId}", indexId);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in LoadOrgList with indexId={IndexId}", indexId);
                throw;
            }
        }

        public async Task<string> UpdateDocument(int docId, int categoryId, string startDate, string endDate, string ic, string userId, string comments)
        {
            try
            {
                var result = await _institutionalFilesRepository.UpdateDocument(docId, categoryId, startDate, endDate, ic, userId, comments);
                Log.Information("Successfully executed UpdateDocument with docId={DocId}, categoryId={CategoryId}, userId={UserId}", docId, categoryId, userId);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in UpdateDocument with docId={DocId}, categoryId={CategoryId}, userId={UserId}", docId, categoryId, userId);
                throw;
            }
        }

        public async Task DisableDoc(int docId, string userId)
        {
            try
            {
                await _institutionalFilesRepository.DisableDoc(docId, userId);
                Log.Information("Successfully executed DisableDoc with docId={DocId}, userId={UserId}", docId, userId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in DisableDoc with docId={DocId}, userId={UserId}", docId, userId);
                throw;
            }
        }

        public async Task<string> GetDocID(int orgId, int categoryId, string fileType, string startDate, string endDate, string ic, string userId, string comments)
        {
            try
            {
                var result = await _institutionalFilesRepository.GetDocID(orgId, categoryId, fileType, startDate, endDate, ic, userId, comments);
                Log.Information("Successfully executed GetDocID with orgId={OrgId}, categoryId={CategoryId}, fileType={FileType}, userId={UserId}", orgId, categoryId, fileType, userId);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in GetDocID with orgId={OrgId}, categoryId={CategoryId}, fileType={FileType}, userId={UserId}", orgId, categoryId, fileType, userId);
                throw;
            }
        }

        public async Task<List<InstitutionalOrg>> SearchOrgList(string search_str)
        {
            try
            {
                var result = await _institutionalFilesRepository.SearchOrgList(search_str);
                Log.Information("Successfully executed SearchOrgList with search_str={SearchStr}", search_str);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in SearchOrgList with search_str={SearchStr}", search_str);
                throw;
            }
        }
    }
}
