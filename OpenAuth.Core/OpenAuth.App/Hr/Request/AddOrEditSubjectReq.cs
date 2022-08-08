using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Hr.Request
{
    public class AddOrEditSubjectReq
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int State { get; set; }
    }
    public class AddOrEditSubjectCourseReq: AddOrEditSubjectReq
    {
        public int SubjectId { get; set; }
        public int Type { get; set; }
        public string Content { get; set; }
    }
}
