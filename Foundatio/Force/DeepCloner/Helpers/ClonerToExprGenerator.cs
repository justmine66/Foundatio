using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Foundatio.Force.DeepCloner.Helpers
{
    internal static class ClonerToExprGenerator
    {
        internal static object GenerateClonerInternal(Type realType, bool isDeepClone)
        {
            if (realType.IsValueType())
            {
                throw new InvalidOperationException("Operation is valid only for reference types");
            }
            return GenerateProcessMethod(realType, isDeepClone);
        }

        private static object GenerateProcessMethod(Type type, bool isDeepClone)
        {
            if (type.IsArray)
            {
                return GenerateProcessArrayMethod(type, isDeepClone);
            }
            Type typeFromHandle = typeof(object);
            List<Expression> list = new List<Expression>();
            ParameterExpression parameterExpression = Expression.Parameter(typeFromHandle);
            ParameterExpression parameterExpression2 = parameterExpression;
            ParameterExpression parameterExpression3 = Expression.Parameter(typeFromHandle);
            ParameterExpression parameterExpression4 = parameterExpression3;
            ParameterExpression parameterExpression5 = Expression.Parameter(typeof(DeepCloneState));
            parameterExpression2 = Expression.Variable(type);
            parameterExpression4 = Expression.Variable(type);
            list.Add(Expression.Assign(parameterExpression2, Expression.Convert(parameterExpression, type)));
            list.Add(Expression.Assign(parameterExpression4, Expression.Convert(parameterExpression3, type)));
            if (isDeepClone)
            {
                list.Add(Expression.Call(parameterExpression5, typeof(DeepCloneState).GetMethod("AddKnownRef"), parameterExpression, parameterExpression3));
            }
            List<FieldInfo> list2 = new List<FieldInfo>();
            Type type2 = type;
            while (!(type2.Name == "ContextBoundObject"))
            {
                list2.AddRange(type2.GetDeclaredFields());
                type2 = type2.BaseType();
                if (!(type2 != (Type)null))
                {
                    break;
                }
            }
            foreach (FieldInfo item in list2)
            {
                if (isDeepClone && !DeepClonerSafeTypes.CanNotCopyType(item.FieldType, null))
                {
                    MethodInfo method = item.FieldType.IsValueType() ? typeof(DeepClonerGenerator).GetPrivateStaticMethod("CloneStructInternal").MakeGenericMethod(item.FieldType) : typeof(DeepClonerGenerator).GetPrivateStaticMethod("CloneClassInternal");
                    MemberExpression arg = Expression.Field(parameterExpression2, item);
                    Expression expression = Expression.Call(method, arg, parameterExpression5);
                    if (!item.FieldType.IsValueType())
                    {
                        expression = Expression.Convert(expression, item.FieldType);
                    }
                    if (item.IsInitOnly)
                    {
                        MethodInfo privateStaticMethod = typeof(DeepClonerExprGenerator).GetPrivateStaticMethod("ForceSetField");
                        list.Add(Expression.Call(privateStaticMethod, Expression.Constant(item), Expression.Convert(parameterExpression4, typeof(object)), Expression.Convert(expression, typeof(object))));
                    }
                    else
                    {
                        list.Add(Expression.Assign(Expression.Field(parameterExpression4, item), expression));
                    }
                }
                else
                {
                    list.Add(Expression.Assign(Expression.Field(parameterExpression4, item), Expression.Field(parameterExpression2, item)));
                }
            }
            list.Add(Expression.Convert(parameterExpression4, typeFromHandle));
            Type delegateType = typeof(Func<,,,>).MakeGenericType(typeFromHandle, typeFromHandle, typeof(DeepCloneState), typeFromHandle);
            List<ParameterExpression> list3 = new List<ParameterExpression>();
            if (parameterExpression != parameterExpression2)
            {
                list3.Add(parameterExpression2);
            }
            if (parameterExpression3 != parameterExpression4)
            {
                list3.Add(parameterExpression4);
            }
            return Expression.Lambda(delegateType, Expression.Block(list3, list), parameterExpression, parameterExpression3, parameterExpression5).Compile();
        }

        private static object GenerateProcessArrayMethod(Type type, bool isDeep)
        {
            Type elementType = type.GetElementType();
            int arrayRank = type.GetArrayRank();
            ParameterExpression parameterExpression = Expression.Parameter(typeof(object));
            ParameterExpression parameterExpression2 = Expression.Parameter(typeof(object));
            ParameterExpression parameterExpression3 = Expression.Parameter(typeof(DeepCloneState));
            Type delegateType = typeof(Func<,,,>).MakeGenericType(typeof(object), typeof(object), typeof(DeepCloneState), typeof(object));
            if (arrayRank == 1 && type == elementType.MakeArrayType())
            {
                if (!isDeep)
                {
                    MethodCallExpression body = Expression.Call(typeof(ClonerToExprGenerator).GetPrivateStaticMethod("ShallowClone1DimArraySafeInternal").MakeGenericMethod(elementType), Expression.Convert(parameterExpression, type), Expression.Convert(parameterExpression2, type));
                    return Expression.Lambda(delegateType, body, parameterExpression, parameterExpression2, parameterExpression3).Compile();
                }
                string methodName = "Clone1DimArrayClassInternal";
                if (DeepClonerSafeTypes.CanNotCopyType(elementType, null))
                {
                    methodName = "Clone1DimArraySafeInternal";
                }
                else if (elementType.IsValueType())
                {
                    methodName = "Clone1DimArrayStructInternal";
                }
                MethodCallExpression body2 = Expression.Call(typeof(ClonerToExprGenerator).GetPrivateStaticMethod(methodName).MakeGenericMethod(elementType), Expression.Convert(parameterExpression, type), Expression.Convert(parameterExpression2, type), parameterExpression3);
                return Expression.Lambda(delegateType, body2, parameterExpression, parameterExpression2, parameterExpression3).Compile();
            }
            MethodCallExpression body3 = Expression.Call(typeof(ClonerToExprGenerator).GetPrivateStaticMethod((arrayRank == 2 && type == elementType.MakeArrayType()) ? "Clone2DimArrayInternal" : "CloneAbstractArrayInternal"), Expression.Convert(parameterExpression, type), Expression.Convert(parameterExpression2, type), parameterExpression3, Expression.Constant(isDeep));
            return Expression.Lambda(delegateType, body3, parameterExpression, parameterExpression2, parameterExpression3).Compile();
        }

        internal static T[] ShallowClone1DimArraySafeInternal<T>(T[] objFrom, T[] objTo)
        {
            int length = Math.Min(objFrom.Length, objTo.Length);
            Array.Copy(objFrom, objTo, length);
            return objTo;
        }

        internal static T[] Clone1DimArraySafeInternal<T>(T[] objFrom, T[] objTo, DeepCloneState state)
        {
            int length = Math.Min(objFrom.Length, objTo.Length);
            state.AddKnownRef(objFrom, objTo);
            Array.Copy(objFrom, objTo, length);
            return objTo;
        }

        internal static T[] Clone1DimArrayStructInternal<T>(T[] objFrom, T[] objTo, DeepCloneState state)
        {
            if (objFrom == null || objTo == null)
            {
                return null;
            }
            int num = Math.Min(objFrom.Length, objTo.Length);
            state.AddKnownRef(objFrom, objTo);
            Func<T, DeepCloneState, T> clonerForValueType = DeepClonerGenerator.GetClonerForValueType<T>();
            for (int i = 0; i < num; i++)
            {
                objTo[i] = clonerForValueType(objTo[i], state);
            }
            return objTo;
        }

        internal static T[] Clone1DimArrayClassInternal<T>(T[] objFrom, T[] objTo, DeepCloneState state)
        {
            if (objFrom == null || objTo == null)
            {
                return null;
            }
            int num = Math.Min(objFrom.Length, objTo.Length);
            state.AddKnownRef(objFrom, objTo);
            for (int i = 0; i < num; i++)
            {
                objTo[i] = (T)DeepClonerGenerator.CloneClassInternal(objFrom[i], state);
            }
            return objTo;
        }

        internal static T[,] Clone2DimArrayInternal<T>(T[,] objFrom, T[,] objTo, DeepCloneState state, bool isDeep)
        {
            if (objFrom == null || objTo == null)
            {
                return null;
            }
            int num = Math.Min(objFrom.GetLength(0), objTo.GetLength(0));
            int num2 = Math.Min(objFrom.GetLength(1), objTo.GetLength(1));
            state.AddKnownRef(objFrom, objTo);
            if ((!isDeep || DeepClonerSafeTypes.CanNotCopyType(typeof(T), null)) && objFrom.GetLength(0) == objTo.GetLength(0) && objFrom.GetLength(1) == objTo.GetLength(1))
            {
                Array.Copy(objFrom, objTo, objFrom.Length);
                return objTo;
            }
            if (!isDeep)
            {
                for (int i = 0; i < num; i++)
                {
                    for (int j = 0; j < num2; j++)
                    {
                        int num3 = i;
                        int num4 = j;
                        T val = objFrom[i, j];
                        objTo[num3, num4] = val;
                    }
                }
                return objTo;
            }
            if (typeof(T).IsValueType())
            {
                Func<T, DeepCloneState, T> clonerForValueType = DeepClonerGenerator.GetClonerForValueType<T>();
                for (int k = 0; k < num; k++)
                {
                    for (int l = 0; l < num2; l++)
                    {
                        int num5 = k;
                        int num6 = l;
                        T val2 = clonerForValueType(objFrom[k, l], state);
                        objTo[num5, num6] = val2;
                    }
                }
            }
            else
            {
                for (int m = 0; m < num; m++)
                {
                    for (int n = 0; n < num2; n++)
                    {
                        int num7 = m;
                        int num8 = n;
                        T obj = (T)DeepClonerGenerator.CloneClassInternal(objFrom[m, n], state);
                        objTo[num7, num8] = obj;
                    }
                }
            }
            return objTo;
        }

        internal static Array CloneAbstractArrayInternal(Array objFrom, Array objTo, DeepCloneState state, bool isDeep)
        {
            if (objFrom == null || objTo == null)
            {
                return null;
            }
            int rank = objFrom.Rank;
            if (objTo.Rank == rank)
            {
                int[] array = Enumerable.Range(0, rank).Select(objFrom.GetLowerBound).ToArray();
                int[] array2 = Enumerable.Range(0, rank).Select(objTo.GetLowerBound).ToArray();
                int[] array3 = (from x in Enumerable.Range(0, rank)
                                select Math.Min(objFrom.GetLength(x), objTo.GetLength(x))).ToArray();
                int[] array4 = Enumerable.Range(0, rank).Select(objFrom.GetLowerBound).ToArray();
                int[] array5 = Enumerable.Range(0, rank).Select(objTo.GetLowerBound).ToArray();
                state.AddKnownRef(objFrom, objTo);
                while (true)
                {
                    if (isDeep)
                    {
                        objTo.SetValue(DeepClonerGenerator.CloneClassInternal(objFrom.GetValue(array4), state), array5);
                    }
                    else
                    {
                        objTo.SetValue(objFrom.GetValue(array4), array5);
                    }
                    int num = rank - 1;
                    while (true)
                    {
                        array4[num]++;
                        array5[num]++;
                        if (array4[num] < array[num] + array3[num])
                        {
                            break;
                        }
                        array4[num] = array[num];
                        array5[num] = array2[num];
                        num--;
                        if (num < 0)
                        {
                            return objTo;
                        }
                    }
                }
            }
            throw new InvalidOperationException("Invalid rank of target array");
        }
    }

}
