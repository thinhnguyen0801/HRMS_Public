using Microsoft.AspNetCore.Components;
using HNOne.Web.Services.Interfaces;

namespace HNOne.Web.Controllers
{
    public class ContractAppendixListController : DocumentControllerBase
    {
        [Inject] IPersonnelService _personnelService { get; init; }
    }
}
