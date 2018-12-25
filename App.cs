using Bridge.Html5;
using MVVM.ViewModels;

namespace MVVM
{
    public static class App
    {
        public static void Main()
        {
            var vm = new PeopleViewModel();
            PeopleViewModel.Focus.Subscribe(arg =>
            {
                Document.GetElementById(arg.NewData)?.Focus();
            });

            var html = new Html(Document.Body);
            html.Div.Text("Test")
                    .Input.Value(vm.FirstName).Id(nameof(vm.FirstName)).End
                    .Input.Value(vm.LastName).End
                    .Button.Text("Add person").Event(EventType.Click, vm.Add).End
                    .Button.Text("Load data").AsyncEvent(EventType.Click, vm.LoadPeople).End
                .End.Render();

            html.Table
                .Theader.TRow
                    .Th.Text("First name").End
                    .Th.Text("Last name").End
                    .Th.Text("Full name").End
                    .Th.Text("Action").End
                .End.End
                .TBody.ForEach(vm.People, (Person person, int index) =>
                {
                    html.TRow.Render();
                    html.TData
                        .Input.Value(person.FirstName).Visible(person.EditMode).End
                        .Span.Text(person.FirstName).Hidden(person.EditMode).End
                    .End
                    .TData
                        .Input.Value(person.LastName).Visible(person.EditMode).End
                        .Span.Text(person.LastName).Hidden(person.EditMode).End
                    .End
                    .TData.Text(person.FullNameCapitalized).End
                    .TData
                        .Button.Text("Edit").Event(EventType.Click, person.Edit).End
                        .Button.Text("X").Event(EventType.Click, vm.Remove, person).End
                    .End.End.Render();
                }).End
            .End.Render();
            PeopleViewModel.Focus.Data = nameof(vm.FirstName);
        }
    }
}