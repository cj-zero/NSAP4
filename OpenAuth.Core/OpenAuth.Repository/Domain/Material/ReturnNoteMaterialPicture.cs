﻿//------------------------------------------------------------------------------
//     This code was generated by a CodeSmith Template.
//
//     DO NOT MODIFY contents of this file. Changes to this
//     file will be lost if the code is regenerated.
//     Author:Yubao Li
//------------------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using OpenAuth.Repository.Core;

namespace OpenAuth.Repository.Domain
{
    /// <summary>
       ///
       /// </summary>
       [Table("returnnotematerialpicture")]
    public class ReturnNoteMaterialPicture : Entity
    {
        public ReturnNoteMaterialPicture()
        {
           this.PictureId="";
       this.ReturnnoteMaterialId="";

        }
    /// <summary>
       ///图片Id
       /// </summary>
       [Description("图片Id")]
       public string PictureId { get; set; }

       /// <summary>
       ///退料物料Id
       /// </summary>
       [Description("退料物料Id")]
       public string ReturnnoteMaterialId { get; set; }

       
    }
}