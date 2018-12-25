using Bridge.Html5;
using System;
using System.Threading.Tasks;

namespace MVVM
{
    public class Html
    {
        public Element Context { get; set; }

        public Html(string selector)
        {
            Context = Document.QuerySelector(selector);
        }

        public Html()
        {
            Context = Document.Body;
        }

        public Html(Element ele)
        {
            Context = ele;
        }


        public Html Div
        {
            get
            {
                return Add(ElementType.div);
            }
        }

        public Html Input
        {
            get
            {
                return Add(ElementType.input);
            }
        }

        public Html Span
        {
            get
            {
                return Add(ElementType.span);
            }
        }

        public Html Button
        {
            get
            {
                return Add(ElementType.button);
            }
        }

        public Html Table
        {
            get
            {
                return Add(ElementType.table);
            }
        }

        public Html Theader
        {
            get
            {
                return Add(ElementType.thead);
            }
        }

        public Html Th
        {
            get
            {
                return Add(ElementType.th);
            }
        }

        public Html TBody
        {
            get
            {
                return Add(ElementType.tbody);
            }
        }

        public Html TRow
        {
            get
            {
                return Add(ElementType.tr);
            }
        }

        public Html TData
        {
            get
            {
                return Add(ElementType.td);
            }
        }

        public Html P
        {
            get
            {
                return Add(ElementType.p);
            }
        }

        public Html TextArea
        {
            get
            {
                return Add(ElementType.textarea);
            }
        }

        public Html Br
        {
            get
            {
                var br = new HTMLBRElement();
                Context.AppendChild(br);
                return this;
            }
        }

        public Html End
        {
            get
            {
                Context = Context.ParentElement;
                return this;
            }
        }

        public void Render()
        {
            // Method intentionally left empty.
        }

        public Html Add(ElementType type)
        {
            var ele = Document.CreateElement(type.ToString());
            Context.AppendChild(ele);
            Context = ele;
            return this;
        }

        public Html Id(string id)
        {
            Context.Id = id;
            return this;
        }

        public Html ClassName(string className)
        {
            Context.ClassName = className;
            return this;
        }

        public Html Value(Observable val)
        {
#pragma warning disable IDE0019 // Use pattern matching
            var input = Context as HTMLInputElement;
#pragma warning restore IDE0019 // Use pattern matching
            var textArea = Context as HTMLTextAreaElement;
            if (input != null)
            {
                input.Value = val.Data.ToString();
                input.OnInput += (e) =>
                {
                    val.Data = input.Value;
                };
                val.Subscribe(arg =>
                {
                    input.Value = arg.NewData.ToString();
                });
            }
            else if (textArea != null)
            {
                textArea.Value = val.Data.ToString();
                textArea.OnInput += (e) =>
                {
                    val.Data = textArea.Value;
                };
                val.Subscribe(arg =>
                {
                    textArea.Value = arg.NewData.ToString();
                });
            }
            return this;
        }

        public Html Text(string val)
        {
            var text = new Text(val);
            Context.AppendChild(text);
            return this;
        }

        public Html Text(Observable val)
        {
            var text = new Text(val.Data.ToString());
            val.Subscribe(arg =>
            {
                text.TextContent = arg.NewData?.ToString();
            });
            Context.AppendChild(text);
            return this;
        }

        public Html Text(Observable<string> val)
        {
            var text = new Text(val.Data);
            val.Subscribe(arg =>
            {
                text.TextContent = arg.NewData;
            });
            Context.AppendChild(text);
            return this;
        }

        public Html Event(EventType type, Action action)
        {
            Context.AddEventListener(type, (e) =>
            {
                action();
            });
            return this;
        }

        public Html AsyncEvent(EventType type, Func<Task> action)
        {
            Context.AddEventListener(type, async delegate(Event e)
            {
                await action();
            });
            return this;
        }

        public Html Event<T>(EventType type, Action<T> action, T model = null) where T : class
        {
            Context.AddEventListener(type, (e) =>
            {
                action(model);
            });
            return this;
        }

        public void ClearContextContent()
        {
            Context.InnerHTML = string.Empty;
        }

        public Html ForEach<T>(ObservableArray<T> observableArray, Action<T, int> renderer)
        {
            if (observableArray == null)
                throw new ArgumentNullException(nameof(observableArray));
            var element = Context;

            var array = observableArray.Data as Array;
            var length = array.Length;
            var index = -1;

            while (++index < length)
            {
                Context = element;
                renderer.Call(element, array[index] ?? index, index);
            }
            observableArray.Subscribe((ObservableArrayArgs<T> e) =>
            {
                e.Element = element;
                e.Renderer = renderer;
                e.Action = e.Action ?? ObservableAction.Render;
                Update(e);
            });
            return this;
        }

        private void Update<T>(ObservableArrayArgs<T> arg)
        {
            var numOfElement = 0;
            Context = arg.Element;
            switch (arg.Action)
            {
                case ObservableAction.Push:
                    arg.Renderer.Call(arg.Element, arg.Item, arg.Index);
                    break;
                case ObservableAction.Add:
                    if (arg.Index == arg.Array.Length - 1)
                    {
                        arg.Renderer.Call(arg.Element, arg.Item, arg.Index);
                        return;
                    }
                    var div = new HTMLDivElement();
                    Context = div;
                    arg.Renderer.Call(div, arg.Item, arg.Index);
                    AppendChildList(arg.Element, div, arg.Index);
                    break;
                case ObservableAction.Remove:
                    numOfElement = arg.Element.Children.Length / (arg.Array.Length + 1);
                    RemoveChildList(arg.Element, arg.Index, numOfElement);
                    break;
                case ObservableAction.Move:
                    numOfElement = arg.Element.Children.Length / arg.Array.Length;
                    var newIndex = arg.Index;
                    var oldIndex = Array.IndexOf(arg.Array, arg.Item);
                    if (newIndex == oldIndex)
                        return;
                    var firstOldElementIndex = oldIndex * numOfElement;
                    var nodeToInsert = oldIndex < newIndex ? arg.Element.Children[(newIndex + 1) * numOfElement] : arg.Element.Children[newIndex * numOfElement];
                    for (var j = 0; j < numOfElement; j++)
                    {
                        arg.Element.InsertBefore(arg.Element.Children[firstOldElementIndex], nodeToInsert);
                        if (oldIndex > newIndex)
                            firstOldElementIndex++;
                    }
                    break;
                case ObservableAction.Render:
                    ClearContextContent();
                    var length = arg.Array.Length;
                    var i = -1;
                    while (++i < length)
                    {
                        Context = arg.Element;
                        arg.Renderer.Call(arg.Element, arg.Array[i], i);
                    }
                    break;
            }
        }

        private void AppendChildList(Element parent, Element tmpNode, int index)
        {
            Element previousNode = null;
            index = index * tmpNode.Children.Length;
            previousNode = parent.Children[index];
            while (tmpNode.Children.Length > 0)
            {
                parent.InsertBefore(tmpNode.Children[0], previousNode);
            }
        }

        private void RemoveChildList(Element parent, int index, int numOfElement)
        {
            var startIndex = index * numOfElement;
            for (var i = 0; i < numOfElement && parent.Children.Length > 0; i++)
            {
                parent.RemoveChild(parent.Children[startIndex]);
            }
        }

        public Html Visible(Observable<bool> visible)
        {
            var ele = Context as HTMLElement;
            ele.Style.Display = visible.Data ? "" : Display.None.ToString();
            visible.Subscribe(arg =>
            {
                ele.Style.Display = arg.NewData ? "" : Display.None.ToString();
            });
            return this;
        }

        public Html Hidden(Observable<bool> hidden)
        {
            var ele = Context as HTMLElement;
            ele.Style.Display = hidden.Data ? Display.None.ToString(): string.Empty;
            hidden.Subscribe(arg =>
            {
                ele.Style.Display = arg.NewData ? Display.None.ToString() : string.Empty;
            });
            return this;
        }
    }
}
