using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArithmeticCalculatorOperationApi.Domain.Models.Response
{
    public class InnerResponse
    {
        public int StatusCode { get; set; }
        public InnerData? Data { get; set; }
    }
}
