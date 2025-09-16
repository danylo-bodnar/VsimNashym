using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;

namespace api.Models
{
    public class UserConversationState
    {
        public ConversationStep Step { get; set; } = ConversationStep.None;
        public string? TempDisplayName { get; set; }
        public int? TempAge { get; set; }

        public Point? TempLocation { get; set; }

        public string? TempProfilePhotoFileId { get; set; }
    }
}