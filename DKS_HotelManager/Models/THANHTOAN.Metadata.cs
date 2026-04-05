using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DKS_HotelManager.Models
{
    [MetadataType(typeof(ThanhtoanMetadata))]
    public partial class THANHTOAN
    {
    }

    public class ThanhtoanMetadata
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaTT { get; set; }
    }
}
