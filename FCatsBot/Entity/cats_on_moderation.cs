//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace FCatsBot.Entity
{
    using System;
    using System.Collections.Generic;
    
    public partial class cats_on_moderation
    {
        public int id { get; set; }
        public long user_id { get; set; }
        public string file_id { get; set; }
        public float watson_percentage { get; set; }
        public float watson_trashhold { get; set; }
        public string watson_jsonanswer { get; set; }
        public System.DateTime datetime_added { get; set; }
        public string from_url { get; set; }
    }
}