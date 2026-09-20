using SqlSugar;
using SystemAdmin.Common.Enums.FormBusiness;
using SystemAdmin.Common.Utilities;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.FormBusiness.Forms.OverseasTripApp.Dto;
using SystemAdmin.Model.FormBusiness.Forms.OverseasTripApp.Entity;
using SystemAdmin.Model.FormBusiness.Forms.PublicForm.Entity;
using SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Entity;
using SystemAdmin.Model.SystemBasicMgmt.SystemConfig.Entity;
using SystemAdmin.Repository.FormBusiness.Workflow;

namespace SystemAdmin.Repository.FormBusiness.Forms
{
    public class OverseasTripAppRepository
    {
        private readonly SqlSugarScope _db;
        private readonly Language _lang;
        private readonly FormManager _formManager;

        public OverseasTripAppRepository(SqlSugarScope db, Language lang, FormManager formManager)
        {
            _db = db;
            _lang = lang;
            _formManager = formManager;
        }

        /// <summary>
        /// 交通方式下拉
        /// </summary>
        /// <returns></returns>
        public async Task<List<TravelModeDropDto>> GetTravelModeDrop()
        {
            return await _db.Queryable<DictionaryInfoEntity>()
                            .With(SqlWith.NoLock)
                            .Where(dic => dic.DicType == "TravelMode")
                            .OrderBy(dic => dic.SortOrder)
                            .Select(dic => new TravelModeDropDto()
                            {
                                TravelMode = dic.DicCode,
                                TravelModeName = _lang.Locale == "zh-CN"
                                                 ? dic.DicNameCn
                                                 : dic.DicNameEn,
                            }).ToListAsync();
        }

        /// <summary>
        /// 厂区下拉
        /// </summary>
        /// <returns></returns>
        public async Task<List<FactoryDropDto>> GetFactoryDrop()
        {
            return await _db.Queryable<DictionaryInfoEntity>()
                            .With(SqlWith.NoLock)
                            .Where(dic => dic.DicType == "Factorys")
                            .OrderBy(dic => dic.SortOrder)
                            .Select(dic => new FactoryDropDto()
                            {
                                Factory = dic.DicCode,
                                FactoryName = _lang.Locale == "zh-CN"
                                              ? dic.DicNameCn
                                              : dic.DicNameEn,
                            }).ToListAsync();
        }

        /// <summary>
        /// 查询表单申请人所在部门的厂区
        /// </summary>
        /// <param name="formId"></param>
        /// <returns></returns>
        public async Task<string?> GetApplicantFactory(long formId)
        {
            return await _db.Queryable<FormInstanceEntity>()
                            .With(SqlWith.NoLock)
                            .InnerJoin<UserInfoEntity>((form, user) => form.ApplicantUserId == user.UserId)
                            .InnerJoin<DepartmentInfoEntity>((form, user, dept) => user.DepartmentId == dept.DepartmentId)
                            .Where((form, user, dept) => form.FormId == formId)
                            .Select((form, user, dept) => dept.Factory)
                            .FirstAsync();
        }

        /// <summary>
        /// 查询表单申请人Id
        /// </summary>
        /// <param name="formId"></param>
        /// <returns></returns>
        public async Task<long> GetApplicantUserId(long formId)
        {
            return await _db.Queryable<FormInstanceEntity>()
                            .With(SqlWith.NoLock)
                            .Where(instance => instance.FormId == formId)
                            .Select(instance => instance.ApplicantUserId)
                            .FirstAsync();
        }

        /// <summary>
        /// 查询当前出差单
        /// </summary>
        /// <param name="formId"></param>
        /// <returns></returns>
        public async Task<OverseasTripAppEntity> GetCurrentOverseasTripApp(long formId)
        {
            return await _db.Queryable<OverseasTripAppEntity>()
                            .With(SqlWith.NoLock)
                            .Where(trip => trip.FormId == formId)
                            .FirstAsync();
        }

        /// <summary>
        /// 查询申请人名下与指定时间段重叠的出差单（排除指定表单、已驳回及已作废的出差单）
        /// </summary>
        /// <param name="applicantUserId"></param>
        /// <param name="excludeFormId"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public async Task<List<TripConflictDto>> GetOverlappingTripConflicts(long applicantUserId, long excludeFormId, DateTime startDate, DateTime endDate)
        {
            return await _db.Queryable<OverseasTripAppEntity>()
                            .With(SqlWith.NoLock)
                            .InnerJoin<FormInstanceEntity>((trip, instance) => trip.FormId == instance.FormId)
                            .Where((trip, instance) => instance.ApplicantUserId == applicantUserId
                                                     && instance.FormId != excludeFormId
                                                     && instance.FormStatus != FormStatus.Rejected.ToEnumString()
                                                     && instance.FormStatus != FormStatus.Voided.ToEnumString()
                                                     && trip.StartDate < endDate && startDate < trip.EndDate)
                            .Select((trip, instance) => new TripConflictDto()
                            {
                                FormNo = instance.FormNo,
                                StartDate = trip.StartDate!.Value,
                                EndDate = trip.EndDate!.Value
                            }).ToListAsync();
        }

        /// <summary>
        /// 初始化出差单
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<int> InitOverseasTripApp(OverseasTripAppEntity entity)
        {
            return await _db.Insertable(entity).ExecuteCommandAsync();
        }

        /// <summary>
        /// 保存出差单
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<int> SaveOverseasTripApp(OverseasTripAppEntity entity)
        {
            var count = await _db.Updateable(entity)
                                 .IgnoreColumns(trip => new
                                 {
                                     trip.FormId,
                                     trip.CreatedBy,
                                     trip.CreatedDate,
                                 }).Where(trip => trip.FormId == entity.FormId)
                                 .ExecuteCommandAsync();

            await _formManager.SaveFormSearch(entity.FormId,
                                              [entity.DepartureFactory, entity.DestinationFactory, entity.TripReason, entity.JobDescription]);

            return count;
        }

        /// <summary>
        /// 查询出差单明细
        /// </summary>
        /// <param name="formId"></param>
        /// <returns></returns>
        public async Task<OverseasTripAppDto> GetOverseasTripApp(long formId)
        {
            return await _db.Queryable<FormInstanceEntity>()
                            .With(SqlWith.NoLock)
                            .InnerJoin<OverseasTripAppEntity>((form, trip) => form.FormId == trip.FormId)
                            .InnerJoin<UserInfoEntity>((form, trip, user) => form.ApplicantUserId == user.UserId)
                            .InnerJoin<DepartmentInfoEntity>((form, trip, user, dept) => user.DepartmentId == dept.DepartmentId)
                            .InnerJoin<DictionaryInfoEntity>((form, trip, user, dept, dic) => dic.DicType == "FormStatus" && form.FormStatus == dic.DicCode)
                            .LeftJoin<DictionaryInfoEntity>((form, trip, user, dept, dic, departureDic) => departureDic.DicType == "Factorys" && trip.DepartureFactory == departureDic.DicCode)
                            .LeftJoin<DictionaryInfoEntity>((form, trip, user, dept, dic, departureDic, destinationDic) => destinationDic.DicType == "Factorys" && trip.DestinationFactory == destinationDic.DicCode)
                            .LeftJoin<DictionaryInfoEntity>((form, trip, user, dept, dic, departureDic, destinationDic, outboundDic) => outboundDic.DicType == "TravelMode" && trip.OutboundTravel == outboundDic.DicCode)
                            .LeftJoin<DictionaryInfoEntity>((form, trip, user, dept, dic, departureDic, destinationDic, outboundDic, returnDic) => returnDic.DicType == "TravelMode" && trip.ReturnTravel == returnDic.DicCode)
                            .Where((form, trip, user, dept, dic, departureDic, destinationDic, outboundDic, returnDic) => form.FormId == formId)
                            .Select((form, trip, user, dept, dic, departureDic, destinationDic, outboundDic, returnDic) => new OverseasTripAppDto()
                            {
                                FormTypeId = form.FormTypeId,
                                RuleId = form.RuleId,
                                CurrentStepId = form.CurrentStepId,
                                FormStatus = form.FormStatus,
                                FormStatusName = _lang.Locale == "zh-CN"
                                                 ? dic.DicNameCn
                                                 : dic.DicNameEn,
                                FormId = form.FormId,
                                FormNo = form.FormNo,
                                ApplicantUserNo = user.UserNo,
                                ApplicantUserName = _lang.Locale == "zh-CN"
                                                 ? user.UserNameCn
                                                 : user.UserNameEn,
                                ApplicantDeptName = _lang.Locale == "zh-CN"
                                                 ? dept.DepartmentNameCn
                                                 : dept.DepartmentNameEn,
                                ApplicantDate = form.ApplicantDate,
                                DepartureFactory = trip.DepartureFactory,
                                DepartureFactoryName = _lang.Locale == "zh-CN"
                                                 ? departureDic.DicNameCn
                                                 : departureDic.DicNameEn,
                                DestinationFactory = trip.DestinationFactory,
                                DestinationFactoryName = _lang.Locale == "zh-CN"
                                                 ? destinationDic.DicNameCn
                                                 : destinationDic.DicNameEn,
                                TripReason = trip.TripReason,
                                StartDate = trip.StartDate,
                                EndDate = trip.EndDate,
                                Days = trip.Days,
                                OutboundTravel = trip.OutboundTravel,
                                OutboundTravelName = _lang.Locale == "zh-CN"
                                                 ? outboundDic.DicNameCn
                                                 : outboundDic.DicNameEn,
                                ReturnTravel = trip.ReturnTravel,
                                ReturnTravelName = _lang.Locale == "zh-CN"
                                                 ? returnDic.DicNameCn
                                                 : returnDic.DicNameEn,
                                JobDescription = trip.JobDescription,
                            }).FirstAsync();
        }
    }
}
