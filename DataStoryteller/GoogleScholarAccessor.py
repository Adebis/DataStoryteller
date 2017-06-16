import sortby_citations_google_scholar

class GoogleScholarAccessor:

    def __init__(self):
        print ("Initializing new GoogleScholarAccessor")

    def update_data(self):
        print ("Updating data")
        directory_path = "C:\\Users\\Zev\\Documents\\GitHub\\DataStoryteller\\DataStoryteller\\DataStoryteller\\"
        sortby_citations_google_scholar.update_citations("'artificial neural network'", 10, directory_path + "artificial_neural_network.csv")