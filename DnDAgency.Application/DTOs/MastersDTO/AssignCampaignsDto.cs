using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DnDAgency.Application.DTOs.MastersDTO
{
    public class AssignCampaignsDto
    {
        public List<Guid> CampaignIds { get; set; } = new();
    }
}
