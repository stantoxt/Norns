﻿using Norns.AOP.Attributes;
using Norns.AOP.Interceptors;

namespace Norns.AOP.Core.Interceptors
{
    [NoIntercept]
    public class InterceptBox : IInterceptBox
    {
        public InterceptBox(IInterceptor interceptor, InterceptPredicate verifier)
        {
            Interceptor = interceptor;
            Verifier = verifier;
        }

        public IInterceptor Interceptor { get; }

        public InterceptPredicate Verifier { get; }
    }
}