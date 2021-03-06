﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using Rubberduck.Parsing.Symbols;
using TYPEATTR = System.Runtime.InteropServices.ComTypes.TYPEATTR;
using IMPLTYPEFLAGS = System.Runtime.InteropServices.ComTypes.IMPLTYPEFLAGS;

namespace Rubberduck.Parsing.ComReflection
{
    public class ComCoClass : ComType, IComTypeWithMembers
    {
        private readonly Dictionary<ComInterface, bool> _interfaces = new Dictionary<ComInterface, bool>();
        private readonly List<ComInterface> _events = new List<ComInterface>();

        public ComInterface DefaultInterface { get; private set; }

        public IEnumerable<ComInterface> EventInterfaces
        {
            get { return _events; }
        }
        public IEnumerable<ComInterface> ImplementedInterfaces
        {
            get { return _interfaces.Keys; }
        }

        public IEnumerable<ComInterface> VisibleInterfaces
        {
            get { return _interfaces.Where(i => !i.Value).Select(i => i.Key); }
        }

        public IEnumerable<ComMember> Members
        {
            get { return ImplementedInterfaces.SelectMany(i => i.Members); }
        }

        public bool WithEvents
        {
            get { return _events.Count > 0; }
        }

        public ComCoClass(ITypeLib typeLib, ITypeInfo info, TYPEATTR attrib, int index) : base (typeLib, attrib, index)
        {
            Type = DeclarationType.ClassModule;
            GetImplementedInterfaces(info, attrib);
            Debug.Assert(attrib.cFuncs == 0);
        }

        private void GetImplementedInterfaces(ITypeInfo info, TYPEATTR typeAttr)
        {
            for (var implIndex = 0; implIndex < typeAttr.cImplTypes; implIndex++)
            {
                int href;
                info.GetRefTypeOfImplType(implIndex, out href);

                ITypeInfo implemented;
                info.GetRefTypeInfo(href, out implemented);

                IntPtr attribPtr;
                implemented.GetTypeAttr(out attribPtr);
                var attribs = (TYPEATTR)Marshal.PtrToStructure(attribPtr, typeof(TYPEATTR));

                ComType inherited;
                ComProject.KnownTypes.TryGetValue(attribs.guid, out inherited);
                var intface = inherited as ComInterface ?? new ComInterface(implemented, attribs);                
                ComProject.KnownTypes.TryAdd(attribs.guid, intface);

                IMPLTYPEFLAGS flags = 0;
                try
                {
                    info.GetImplTypeFlags(implIndex, out flags);
                }
                catch (COMException) { }

                DefaultInterface = flags.HasFlag(IMPLTYPEFLAGS.IMPLTYPEFLAG_FDEFAULT) ? intface : DefaultInterface;
                if (flags.HasFlag(IMPLTYPEFLAGS.IMPLTYPEFLAG_FSOURCE))
                {
                    _events.Add(intface);
                }
                _interfaces.Add(intface, flags.HasFlag(IMPLTYPEFLAGS.IMPLTYPEFLAG_FRESTRICTED));
                info.ReleaseTypeAttr(attribPtr);
            }
        }
    }
}
