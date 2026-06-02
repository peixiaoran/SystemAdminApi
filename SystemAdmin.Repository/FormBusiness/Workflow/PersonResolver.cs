using SqlSugar;
using SystemAdmin.CommonSetup.Options;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.FormBusiness.Workflow.PersonResolver.Dto;

namespace SystemAdmin.Repository.FormBusiness.Workflow
{
    public class PersonResolver
    {
        private readonly CurrentUser _loginuser;
        private readonly SqlSugarScope _db;
        private readonly Language _lang;

        public PersonResolver(CurrentUser loginuser, SqlSugarScope db, Language lang)
        {
            _loginuser = loginuser;
            _db = db;
            _lang = lang;
        }

        #region 请假单

        /// <summary>
        /// 
        /// </summary>
        /// <param name="formId"></param>
        /// <returns></returns>
        public async Task<CustomUser> Ran(long formId)
        {
            return new CustomUser();
        }

        #endregion
    }
}
