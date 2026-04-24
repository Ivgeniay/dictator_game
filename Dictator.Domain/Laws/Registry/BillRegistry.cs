using System.Collections.Generic;
using Dictator.Domain.Utils.Serd;
using Dictator.Domain.Laws.Bill_;
using Newtonsoft.Json;
using System.Linq;
using System;

namespace Dictator.Domain.Laws.Registry
{
    public class BillRegistry
    {
        public Guid Id { get; }
        private readonly List<Bill> _bills;
        public IReadOnlyList<Bill> Bills => _bills;

        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            Converters = { new NodeJsonConverter() },
            Formatting = Formatting.Indented
        };

        public BillRegistry(Guid id)
        {
            Id = id;
            _bills = new List<Bill>();
        }

        public void Add(Bill bill)
        {
            _bills.Add(bill);
        }

        public Bill GetById(Guid id)
        {
            return _bills.FirstOrDefault(b => b.Id == id);
        }

        public IReadOnlyList<Bill> GetByStatus(BillStatus status)
        {
            return _bills.Where(b => b.Status == status).ToList();
        }

        public IReadOnlyList<Bill> GetAll()
        {
            return _bills.ToList();
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this, Settings);
        }

        public static BillRegistry? Deserialize(string json)
        {
            return JsonConvert.DeserializeObject<BillRegistry>(json, Settings);
        }
    }
}