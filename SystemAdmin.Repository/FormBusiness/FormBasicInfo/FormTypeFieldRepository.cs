using SqlSugar;
using SystemAdmin.CommonSetup.Options;
using SystemAdmin.Model.FormBusiness.FormBasicInfo.Entity;

namespace SystemAdmin.Repository.FormBusiness.FormBasicInfo
{
    public class FormTypeFieldRepository
    {
        private readonly SqlSugarScope _db;
        private readonly Language _lang;

        public FormTypeFieldRepository(SqlSugarScope db, Language lang)
        {
            _db = db;
            _lang = lang;
        }

        /// <summary>
        /// 新增表单类别栏位
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<int> InsertFormTypeField(FormTypeFieldEntity entity)
        {
            return await _db.Insertable(entity).ExecuteCommandAsync();
        }
    }
}
