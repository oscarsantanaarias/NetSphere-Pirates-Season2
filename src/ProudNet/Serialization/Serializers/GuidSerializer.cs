using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BlubLib.Serialization;
using Sigil;
using Sigil.NonGeneric;

namespace ProudNet.Serialization.Serializers
{
    // BlubLib resolves Guid.ToByteArray() by name only. .NET 9 added a
    // ToByteArray(bool bigEndian) overload, so that lookup now finds two
    // candidates and throws AmbiguousMatchException, which kills every message
    // carrying a Guid. Same 16 raw bytes on the wire as the built-in one.
    public class GuidSerializer : ISerializerCompiler
    {
        public bool CanHandle(Type type)
        {
            return type == typeof(Guid);
        }

        // BlubLib keeps Guid in its primitive compiler table, so an added
        // compiler never wins. The field is readonly but the dictionary behind
        // it is not, so swapping the entry covers every type it serializes,
        // including the SimpleRmi ones we cannot put an attribute on.
        public static void Install()
        {
            var field = typeof(Serializer).GetField("s_primitiveCompiler", BindingFlags.Static | BindingFlags.NonPublic);
            var table = field?.GetValue(null) as IDictionary<Type, ISerializerCompiler>;
            if (table != null)
                table[typeof(Guid)] = new GuidSerializer();
        }

        public static Guid Read(BinaryReader reader)
        {
            return new Guid(reader.ReadBytes(16));
        }

        public static void Write(BinaryWriter writer, Guid value)
        {
            writer.Write(value.ToByteArray());
        }

        public void EmitDeserialize(Emit emiter, Local value)
        {
            emiter.LoadArgument(1);
            emiter.Call(typeof(GuidSerializer).GetMethod(nameof(Read)));
            emiter.StoreLocal(value);
        }

        public void EmitSerialize(Emit emiter, Local value)
        {
            emiter.LoadArgument(1);
            emiter.LoadLocal(value);
            emiter.Call(typeof(GuidSerializer).GetMethod(nameof(Write)));
        }
    }
}
