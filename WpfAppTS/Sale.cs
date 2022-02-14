using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfAppTS {
    public class Sale {
        public Sale(string model, DateTime date, double price, double dph) {
            Model = model;
            Date = date;
            Price = price;
            Dph = dph;
        }

        public string Model { get; set; }
            public DateTime Date { get; set; }
            public double Price { get; set; }
            public double Dph { get; set; }
        
    }
}
