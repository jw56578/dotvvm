using Newtonsoft.Json.Linq;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DotVVM.Framework.Utils
{
    public static class JsonUtils
    {
        public static JObject Diff(JObject source, JObject target, bool nullOnRemoved = false)
        {
            var diff = new JObject();
            foreach (var item in target) {
                var sourceItem = source[item.Key];
                if (sourceItem == null) {
                    if (item.Value != null) {
                        diff[item.Key] = item.Value;
                    }
                } else if (sourceItem.Type != item.Value.Type) {
                    if (sourceItem.Type == JTokenType.Object || sourceItem.Type == JTokenType.Array
                        || item.Value.Type == JTokenType.Object || item.Value.Type == JTokenType.Array
                        || item.Value.ToString() != sourceItem.ToString()) {

                        diff[item.Key] = item.Value;
                    }
                } else if (sourceItem.Type == JTokenType.Object) // == item.Value.Type
                  {
                    var itemDiff = Diff((JObject)sourceItem, (JObject)item.Value, nullOnRemoved);
                    if (itemDiff.Count > 0) {
                        diff[item.Key] = itemDiff;
                    }
                } else if (sourceItem.Type == JTokenType.Array) {
                    var sourceArr = (JArray)sourceItem;
                    var subchanged = false;
                    var arrDiff = Diff(sourceArr, (JArray)item.Value, out subchanged, nullOnRemoved);
                    if (subchanged) {
                        diff[item.Key] = arrDiff;
                    }
                } else if (!JToken.DeepEquals(sourceItem, item.Value)) {
                    diff[item.Key] = item.Value;
                }
            }
            if (nullOnRemoved) {
                foreach (var item in source) {
                    if (target[item.Key] == null) diff[item.Key] = JValue.CreateNull();
                }
            }
            // remove abandoned $options
            foreach (var item in Enumerable.ToArray<KeyValuePair<string, JToken>>(diff)) {
                if (item.Key.EndsWith("$options", StringComparison.Ordinal)) {
                    if (diff[item.Key.Remove(item.Key.Length - "$options".Length)] == null) {
                        diff.Remove(item.Key);
                    }
                }
            }
            return diff;
        }

        public static JArray Diff(JArray source, JArray target, out bool changed, bool nullOnRemoved = false)
        {
            changed = source.Count != target.Count;
            var diffs = new JToken[target.Count];
            var commonLen = Math.Min(diffs.Length, source.Count);
            for (int i = 0; i < commonLen; i++) {
                if (target[i].Type == JTokenType.Object && source[i].Type == JTokenType.Object) {
                    diffs[i] = Diff((JObject)source[i], (JObject)target[i], nullOnRemoved);
                    if (((JObject)diffs[i]).Count > 0) changed = true;
                } else if (target[i].Type == JTokenType.Array && source[i].Type == JTokenType.Array) {
                    var subchanged = false;
                    diffs[i] = Diff((JArray)source[i], (JArray)target[i], out subchanged, nullOnRemoved);
                    if (subchanged) changed = true;
                } else {
                    diffs[i] = target[i];
                    if (!JToken.DeepEquals(source[i], target[i]))
                        changed = true;
                }
            }
            for (int i = commonLen; i < diffs.Length; i++) {
                diffs[i] = target[i];
                changed = true;
            }
            return new JArray(diffs);
        }

        private static JToken PatchItem(JToken target, JToken diff, bool removeOnNull = false)
        {
            if (target.Type == JTokenType.Object && diff.Type == JTokenType.Object) {
                Patch((JObject)target, (JObject)diff, removeOnNull);
                return target;
            } else if (target.Type == JTokenType.Array && diff.Type == JTokenType.Array) {
                return new JArray(((JArray)target).ZipOverhang((JArray)diff, (t, d) => PatchItem(t, d, removeOnNull)).ToArray());
            } else {
                return diff;
            }
        }

        public static void Patch(JObject target, JObject diff, bool removeOnNull = false)
        {
            foreach (var prop in diff) {
                var val = target[prop.Key];
                if (val == null) target[prop.Key] = prop.Value;
                else if (prop.Value.Type == JTokenType.Null && removeOnNull || (prop.Value as JConstructor)?.Name == "$rm") target.Remove(prop.Key);
                else target[prop.Key] = PatchItem(val, prop.Value, removeOnNull);
            }
        }

        public static IEnumerable<T> ZipOverhang<T>(this IEnumerable<T> a, IEnumerable<T> b, Func<T, T, T> combine)
        {
            bool am, bm;
            using (var ae = a.GetEnumerator()) using (var be = b.GetEnumerator())
            {
                while((am = ae.MoveNext()) & (bm = be.MoveNext()))
                {
                    yield return combine(ae.Current, be.Current);
                }
                while (am) { yield return ae.Current; am = ae.MoveNext(); }
                while (bm) { yield return be.Current; bm = be.MoveNext(); }
            }
        }
    }
}
