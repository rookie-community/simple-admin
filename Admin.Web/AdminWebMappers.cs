using Riok.Mapperly.Abstractions;

namespace Admin.Web
{
    [Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
    public partial class AdminWebMappers
    //public partial class AdminWebMappers : MapperBase<BookDto, CreateUpdateBookDto>
    {
        //public override partial CreateUpdateBookDto Map(BookDto source);

        //public override partial void Map(BookDto source, CreateUpdateBookDto destination);
    }
}
