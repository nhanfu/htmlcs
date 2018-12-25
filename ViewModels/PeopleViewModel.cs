using System;
using System.Threading.Tasks;

namespace MVVM.ViewModels
{
    public class PeopleViewModel
    {
        public Observable<string> FirstName { get; private set; } = Observable.Of(string.Empty);
        public Observable<string> LastName { get; private set; } = Observable.Of(string.Empty);
        public ObservableArray<Person> People { get; private set; } = Observable.Of(new Person[] { });
        public static Observable<string> Focus { get; private set; } = Observable.Of(string.Empty);

        public void Add()
        {
            var person = new Person
            {
                FirstName = Observable.Of(FirstName.Data),
                LastName = Observable.Of(LastName.Data)
            };
            People.Add(person, 0);
            FirstName.Data = string.Empty;
            LastName.Data = string.Empty;
            Focus.Data = string.Empty;
            Focus.Data = nameof(FirstName);
        }

        public void Remove(Person person)
        {
            People.Remove(person);
        }

        public async Task LoadPeople()
        {
            var task1 = Test();
            var task2 = Test();
            Console.WriteLine("Task start");
            await Task.WhenAll(task1, task2);
            People.AddRange(task1.Result);
            People.AddRange(task2.Result);
            Console.WriteLine("Task End");
        }

        private async Task<Person[]> Test()
        {
            var nhan = new Person
            {
                FirstName = Observable.Of("Nhan"),
                LastName = Observable.Of("Nguyen")
            };
            var lanAnh = new Person
            {
                FirstName = Observable.Of("Lan Anh"),
                LastName = Observable.Of("Nguyen")
            };
            await Task.Delay(1000);
            return new[] { nhan, lanAnh };
        }
    }
}
