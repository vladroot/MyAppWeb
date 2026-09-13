using System.Text.Json;
using System.Text.Json.Serialization;

namespace MyAppWeb
{
    public class SaveLoadController : ISaveLoad
    {
        class BoolData
        {
            public bool value;
        }

        public class Item
        {
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)] public string? Name;
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)] public int Weight;
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)] public int Price;
        }

        public class SaveLoad
        {
            public string? Name { get; set; }
            public int Age { get; set; }
            public int Weight { get; set; }
            public int Money { get; set; }
            public string[]? Items { get; set; }
        }

        private readonly List<BoolData> _dataList;
        private SaveLoad _saveLoad;

        public SaveLoadController()
        {
            _dataList =
            [
                new BoolData { value = true },
                new BoolData { value = false },
                new BoolData { value = false },
                new BoolData { value = true },
                new BoolData { value = true },
                new BoolData { value = false },
                new BoolData { value = true },
                new BoolData { value = false },
                new BoolData { value = true },
                new BoolData { value = true },
            ];

            _saveLoad = new SaveLoad
            {
                Name = "John Doe",
                Age = 30,
                Money = 10,
                Weight = 84,
                Items =
                [
                    "Sword", "Gun", "Pickaxe", "Bubble"
                ]
            };
        }

        public string DoWork()
        {
            var active = _dataList.Where(e => e.value).Select(e => e.value);

            JsonSerializerOptions options = new() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault };
            var json = JsonSerializer.Serialize<SaveLoad>(_saveLoad, options);
            Console.WriteLine($"JSON:{json}");
            var newObject = JsonSerializer.Deserialize<SaveLoad>(json);
            return json;
        }
    }
}