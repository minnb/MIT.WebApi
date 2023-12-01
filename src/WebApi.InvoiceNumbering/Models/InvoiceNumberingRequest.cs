using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebApi.InvoiceNumbering.Models
{
    public class InvoiceNumberingRequest
    {
        [Required]
        [DefaultValue("201801")]
        public string PosNo { get; set; } = "201801";
        [Required]
        [DefaultValue("0104918404-002")]
        public string TaxCode { get; set; } = "0104918404-002";
        [Required]
        [DefaultValue("HNI-NOR")]
        public string TemplateNo { get; set; } = "HNI-NOR";
        [Required]
        [DefaultValue("K22TBP")]
        public string SerialNo { get; set; } = "K22TBP";

    }
}
