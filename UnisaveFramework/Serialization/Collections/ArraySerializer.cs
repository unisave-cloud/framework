using System;
using LightJson;
using Unisave.Serialization.Context;

namespace Unisave.Serialization.Collections
{
    internal static class ArraySerializer
    {
        public static JsonValue ToJson(
            object subject,
            Type typeScope,
            SerializationContext context
        )
        {
            // handle multi-dimensionality via recursion
            return SerializeNdArray(
                indices: Array.Empty<int>(),
                array: (Array)subject,
                elementTypeScope: typeScope.GetElementType()
                    ?? throw new ArgumentException(
                        $"Given type {typeScope} is not an array."
                    ),
                context: context
            );
        }
        
        // constructs the JsonArray at a level where indices are specified
        // up a certain depth
        private static JsonArray SerializeNdArray(
            int[] indices,
            Array array,
            Type elementTypeScope,
            SerializationContext context
        )
        {
            // invalid arguments
            if (indices.Length >= array.Rank)
                throw new ArgumentException(
                    "Provided indices must be at least one less than the" +
                    "rank of the array."
                );

            // in the last dimension, subIndices are full indices to
            // a specific element, and we can serialize elements,
            // otherwise we need to run recursion on ourselves
            // one dimension lower
            bool inLastDimension = (indices.Length == array.Rank - 1);

            JsonArray subArray = new JsonArray();
            int subLength = array.GetLength(indices.Length);
            
            int[] subIndices = new int[indices.Length + 1];
            Array.Copy(indices, subIndices, indices.Length);
            ref int i = ref subIndices[subIndices.Length - 1]; // last one
            
            for (i = 0; i < subLength; i++)
            {
                if (inLastDimension)
                {
                    // serialize final elements
                    subArray.Add(
                        Serializer.ToJson(
                            array.GetValue(subIndices),
                            elementTypeScope,
                            context
                        )
                    );
                }
                else
                {
                    // recursion one dimension deeper
                    subArray.Add(
                        SerializeNdArray(
                            subIndices,
                            array,
                            elementTypeScope,
                            context
                        )
                    );
                }
            }

            return subArray;
        }

        public static object FromJson(
            JsonValue json,
            Type typeScope,
            DeserializationContext context
        )
        {
            JsonArray jsonArray = json.AsJsonArray;
            if (jsonArray == null)
                return null;

            Type elementTypeScope = typeScope.GetElementType()
                ?? throw new ArgumentException(
                    $"Given type {typeScope} is not an array."
                );

            // get the shape of the array
            int rank = typeScope.GetArrayRank();
            int[] shape = GetShapeRecursively(
                array: jsonArray,
                remainingRank: rank
            );
            
            // allocate the output array with the correct shape
            Array array = Array.CreateInstance(elementTypeScope, shape);
            
            // fill out the allocated array
            DeserializeNdArray(
                indices: Array.Empty<int>(),
                jsonArray: jsonArray,
                array: array,
                elementTypeScope: elementTypeScope,
                context: context
            );
            
            // and return the array
            return array;
        }
        
        private static int[] GetShapeRecursively(
            JsonArray array,
            int remainingRank
        )
        {
            if (remainingRank == 1)
            {
                return new int[1] { array.Count };
            }
            else if (remainingRank > 1)
            {
                if (array.Count == 0)
                {
                    // zeros in all remaining dimensions
                    return new int[remainingRank];
                }
                    
                JsonArray inspectedSubArray = array[0].AsJsonArray;
                if (inspectedSubArray == null)
                    throw new UnisaveSerializationException(
                        "Deserializing an N-dimensional array and hit an " +
                        "unexpected non-JSON-array item too shallow."
                    );
                    
                int[] subShape = GetShapeRecursively(
                    inspectedSubArray,
                    remainingRank - 1
                );
                int[] shape = new int[remainingRank];
                Array.Copy(subShape, 0, shape, 1, subShape.Length);
                shape[0] = array.Count;
                return shape;
            }
            else
            {
                throw new ArgumentException(
                    "Remaining rank must be at least one."
                );
            }
        }

        private static void DeserializeNdArray(
            int[] indices,
            JsonArray jsonArray,
            Array array,
            Type elementTypeScope,
            DeserializationContext context
        )
        {
            // invalid arguments
            if (indices.Length >= array.Rank)
                throw new ArgumentException(
                    "Provided indices must be at least one less than the" +
                    "rank of the array."
                );

            // in the last dimension, subIndices are full indices to
            // a specific element, and we can deserialize elements,
            // otherwise we need to run recursion on ourselves
            // one dimension lower
            bool inLastDimension = (indices.Length == array.Rank - 1);
            
            int subLength = array.GetLength(indices.Length);

            if (jsonArray.Count != subLength)
                throw new UnisaveSerializationException(
                    $"Deserializing N-d array and expecting the sub-array " +
                    $"at index [{string.Join(",", indices)}...] to have length " +
                    $"{subLength}, but it has length {jsonArray.Count} instead."
                );
            
            int[] subIndices = new int[indices.Length + 1];
            Array.Copy(indices, subIndices, indices.Length);
            ref int i = ref subIndices[subIndices.Length - 1]; // last one
            
            for (i = 0; i < subLength; i++)
            {
                if (inLastDimension)
                {
                    // deserialize final elements
                    array.SetValue(
                        Serializer.FromJson(
                            jsonArray[i],
                            elementTypeScope,
                            context
                        ),
                        subIndices
                    );
                }
                else
                {
                    JsonArray subJsonArray = jsonArray[i].AsJsonArray;
                    
                    if (subJsonArray == null)
                        throw new UnisaveSerializationException(
                            $"Deserializing N-d array and found unexpected " +
                            $"non-JSON-array object {jsonArray[i].Type} at index " +
                            $"[{string.Join(",", subIndices)}...] instead of " +
                            $"a sub-array."
                        );
                    
                    // recursion one dimension deeper
                    DeserializeNdArray(
                        subIndices,
                        subJsonArray,
                        array,
                        elementTypeScope,
                        context
                    );
                }
            }
        }
    }
}