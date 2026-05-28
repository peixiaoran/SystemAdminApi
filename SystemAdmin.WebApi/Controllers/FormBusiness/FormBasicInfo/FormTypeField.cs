using Microsoft.AspNetCore.Mvc;
using SystemAdmin.Service.FormBusiness.FormBasicInfo;
using SystemAdmin.WebApi.Attributes;

namespace SystemAdmin.WebApi.Controllers.FormBusiness.FormBasicInfo
{
    [JwtAuthorize]
    [RoutingAuthorize]
    [Route("api/FormBusiness/FormBasicInfo/[controller]/[action]")]
    [ApiController]
    public class FormTypeField : ControllerBase
    {
        private readonly FormTypeFieldService _formTypeFieldService;
        public FormTypeField(FormTypeFieldService formTypeFieldService)
        {
            _formTypeFieldService = formTypeFieldService;
        }
    }
}
