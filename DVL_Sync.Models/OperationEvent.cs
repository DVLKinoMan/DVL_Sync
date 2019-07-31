using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace FakeNamespace
{
    public enum EventType
    {
        Create,
        Edit,
        Delete,
        Rename
    }

    public enum FileType
    {
        File,
        Directory,
    }

    [JsonConverter(typeof(OperationEventConverter))]
    public abstract class OperationEvent
    {
        public abstract EventType EventType { get; set; }

        public string FileName => Path.GetFileName(FilePath);

        public FileType FileType => Directory.Exists(FilePath) ? FileType.Directory : FileType.File;

        public DateTime RaisedTime { get; set; }
        public string FilePath { get; set; }

        public override string ToString() =>
            $"EventType: {EventType} FileName: {FileName} FileType: {FileType} RaisedTime: {RaisedTime} FilePath: {FilePath}";
    }

    //public sealed class DefaultOperationEvent : OperationEvent
    //{

    //}

    public sealed class CreateOperationEvent : OperationEvent
    {
        public override EventType EventType { get { return EventType.Create; } set { } }
    }

    public sealed class EditOperationEvent : OperationEvent
    {
        public override EventType EventType { get { return EventType.Edit; } set { } }
    }

    public sealed class DeleteOperationEvent : OperationEvent
    {
        public override EventType EventType { get { return EventType.Delete; } set { } }
    }

    public sealed class RenameOperationEvent : OperationEvent
    {
        public string OldFilePath { get; set; }
        public string OldFileName => Path.GetFileName(OldFilePath);

        public override EventType EventType { get { return EventType.Rename; } set { } }

        public override string ToString() => $"{base.ToString()} OldFileName: {OldFileName} OldFilePath: {OldFilePath}";
    }

    public class OperationEventConverter : JsonConverter
    {
        //static JsonSerializerSettings SpecifiedSubclassConversion = new JsonSerializerSettings() { ContractResolver = new BaseSpecifiedConcreteClassConverter() };

        public override bool CanConvert(Type objectType) => objectType == typeof(OperationEvent);
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jo = JObject.Load(reader);
            switch (jo["EventType"].Value<int>())
            {
                case 0:
                    return JsonConvert.DeserializeObject<CreateOperationEvent>(jo.ToString());//, SpecifiedSubclassConversion);
                case 1:
                    return JsonConvert.DeserializeObject<EditOperationEvent>(jo.ToString());//, SpecifiedSubclassConversion);
                case 2:
                    return JsonConvert.DeserializeObject<DeleteOperationEvent>(jo.ToString());//, SpecifiedSubclassConversion);
                case 3:
                    return JsonConvert.DeserializeObject<RenameOperationEvent>(jo.ToString());//, SpecifiedSubclassConversion);
                default:
                    throw new Exception();
            }
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => serializer.Serialize(writer, value, typeof(OperationEvent));
    }
}
