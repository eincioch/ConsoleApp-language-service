using Azure;
using System;
using Azure.AI.TextAnalytics;
using System.Security.Principal;
using System.Net;
using Azure.AI.Language.QuestionAnswering;
using Azure.AI.Language.Conversations;
using System.Text.Json;
using Azure.Core;

namespace Example
{
    class Program
    {
        // This example requires environment variables named "LANGUAGE_KEY" and "LANGUAGE_ENDPOINT"
        static string languageKey = "xxxx"; // Environment.GetEnvironmentVariable("LANGUAGE_KEY");
        static string languageEndpoint = "https://---.cognitiveservices.azure.com/"; // Environment.GetEnvironmentVariable("LANGUAGE_ENDPOINT");

        private static readonly AzureKeyCredential credentials = new AzureKeyCredential(languageKey);
        private static readonly Uri endpoint = new Uri(languageEndpoint);

        static async Task Summarization(ConversationAnalysisClient client)
        {
            var data = new
            {
                analysisInput = new
                {
                    conversations = new[]
                    {
                        new
                        {
                            conversationItems = new[]
                            {
                                new
                                {
                                    text = "Hola, estás chateando con René. ¿En qué puedo ayudarte?",
                                    id = "1",
                                    role = "Agent",
                                    participantId = "Agent",
                                },
                                new
                                {
                                    text = "Hola, intenté configurar la conexión wifi para la cafetera Smart Brew 300, pero no funcionó.",
                                    id = "2",
                                    role = "Customer",
                                    participantId = "Customer",
                                },
                                new
                                {
                                    text = @"Lamento escuchar eso. Veamos qué podemos hacer para solucionar este problema. ¿Podrías intentar los siguientes pasos? Primero, ¿podrías presionar el botón de conexión wifi, mantenerlo presionado durante 3 segundos y luego avisarme si la luz de encendido parpadea lentamente cada segundo?",
                                    id = "3",
                                    role = "Agent",
                                    participantId = "Agent",
                                },
                                new
                                {
                                    text = "Sí, presioné el botón de conexión wifi y ahora la luz de encendido parpadea lentamente.",
                                    id = "4",
                                    role = "Customer",
                                    participantId = "Customer",
                                },
                                new
                                {
                                    text = "¡Genial! ¡Gracias! Ahora, verifique en su aplicación Contoso Coffee. ¿Le solicita que se conecte con la máquina?",
                                    id = "5",
                                    role = "Agent",
                                    participantId = "Agent",
                                },
                                new
                                {
                                    text = "No. No pasó nada.",
                                    id = "6",
                                    role = "Customer",
                                    participantId = "Customer",
                                },
                                new
                                {
                                    text = "Lamento mucho escuchar eso. Déjame ver si hay otra manera de solucionar el problema. Espera un minuto.",
                                    id = "7",
                                    role = "Agent",
                                    participantId = "Agent",
                                }
                            },
                            id = "1",
                            language = "es",
                            modality = "text",
                        },
                    }
                },
                tasks = new[]
                {
                    new
                    {
                        parameters = new
                        {
                            summaryAspects = new[]
                            {
                                "issue",
                                "resolution",
                            }
                        },
                        kind = "ConversationalSummarizationTask",
                        taskName = "1",
                    },
                },
            };

            Operation<BinaryData> analyzeConversationOperation = await client.AnalyzeConversationsAsync(WaitUntil.Started, RequestContent.Create(data));
            analyzeConversationOperation.WaitForCompletion();

            using JsonDocument result = JsonDocument.Parse(analyzeConversationOperation.Value.ToStream());
            JsonElement jobResults = result.RootElement;
            foreach (JsonElement task in jobResults.GetProperty("tasks").GetProperty("items").EnumerateArray())
            {
                JsonElement results = task.GetProperty("results");

                Console.WriteLine("Conversations:");
                foreach (JsonElement conversation in results.GetProperty("conversations").EnumerateArray())
                {
                    Console.WriteLine($"Conversation: #{conversation.GetProperty("id").GetString()}");
                    Console.WriteLine("Summaries:");
                    foreach (JsonElement summary in conversation.GetProperty("summaries").EnumerateArray())
                    {
                        Console.WriteLine($"Text: {summary.GetProperty("text").GetString()}");
                        Console.WriteLine($"Aspect: {summary.GetProperty("aspect").GetString()}");
                    }
                    Console.WriteLine();
                }
            }
        }

        // Example method for summarizing text
        static async Task TextSummarizationExample(TextAnalyticsClient client)
        {
            string document = @"En un pueblito no muy lejano, vivía una mamá cerdita junto con sus tres cerditos. Todos eran muy felices hasta que un día la mamá cerdita les dijo:

—Hijitos, ustedes ya han crecido, es tiempo de que sean cerditos adultos y vivan por sí mismos.

Antes de dejarlos ir, les dijo:

—En el mundo nada llega fácil, por lo tanto, deben aprender a trabajar para lograr sus sueños.

Mamá cerdita se despidió con un besito en la mejilla y los tres cerditos se fueron a vivir en el mundo.

El cerdito menor, que era muy, pero muy perezoso, no prestó atención a las palabras de mamá cerdita y decidió construir una casita de paja para terminar temprano y acostarse a descansar.

El cerdito del medio, que era medio perezoso, medio prestó atención a las palabras de mamá cerdita y construyó una casita de palos. La casita le quedó chueca porque como era medio perezoso no quiso leer las instrucciones para construirla.

La cerdita mayor, que era la más aplicada de todos, prestó mucha atención a las palabras de mamá cerdita y quiso construir una casita de ladrillos. La construcción de su casita le tomaría mucho más tiempo. Pero esto no le importó; su nuevo hogar la albergaría del frío y también del temible lobo feroz...

Y hablando del temible lobo feroz, este se encontraba merodeando por el bosque cuando vio al cerdito menor durmiendo tranquilamente a través de su ventana. Al lobo le entró un enorme apetito y pensó que el cerdito sería un muy delicioso bocadillo, así que tocó a la puerta y dijo:

—Cerdito, cerdito, déjame entrar.

El cerdito menor se despertó asustado y respondió:

—¡No, no y no!, nunca te dejaré entrar.

El lobo feroz se enfureció y dijo:

Soplaré y resoplaré y tu casa derribaré.

El lobo sopló y resopló con todas sus fuerzas y la casita de paja se vino al piso. Afortunadamente, el cerdito menor había escapado hacia la casa del cerdito del medio mientras el lobo seguía soplando.

El lobo feroz sintiéndose engañado, se dirigió a la casa del cerdito del medio y al tocar la puerta dijo:

—Cerdito, cerdito, déjame entrar.

El cerdito del medio respondió:

— ¡No, no y no!, nunca te dejaré entrar.

El lobo hambriento se enfureció y dijo:

—Soplaré y resoplaré y tu casa derribaré.

El lobo sopló y resopló con todas sus fuerzas y la casita de palo se vino abajo. Por suerte, los dos cerditos habían corrido hacia la casa de la cerdita mayor mientras que el lobo feroz seguía soplando y resoplando. Los dos hermanos, casi sin respiración le contaron toda la historia.

—Hermanitos, hace mucho frío y ustedes la han pasado muy mal, así que disfrutemos la noche al calor de la fogata —dijo la cerdita mayor y encendió la chimenea. Justo en ese momento, los tres cerditos escucharon que tocaban la puerta.

—Cerdita, cerdita, déjame entrar —dijo el lobo feroz.

La cerdita respondió:

— ¡No, no y no!, nunca te dejaré entrar.

El lobo hambriento se enfureció y dijo:

—Soplaré y soplaré y tu casa derribaré.

El lobo sopló y resopló con todas sus fuerzas, pero la casita de ladrillos resistía sus soplidos y resoplidos. Más enfurecido y hambriento que nunca decidió trepar el techo para meterse por la chimenea. Al bajar la chimenea, el lobo se quemó la cola con la fogata.

—¡AY! —gritó el lobo.

Y salió corriendo por el bosque para nunca más ser visto.

Un día cualquiera, mamá cerdita fue a visitar a sus queridos cerditos y descubrió que todos tres habían construido casitas de ladrillos. Los tres cerditos habían aprendido la lección:

“En el mundo nada llega fácil, por lo tanto, debemos trabajar para lograr nuestros sueños”.";

            // Prepare analyze operation input. You can add multiple documents to this list and perform the same
            // operation to all of them.
            var batchInput = new List<string>
            {
                document
            };

            TextAnalyticsActions actions = new TextAnalyticsActions()
            {
                ExtractiveSummarizeActions = new List<ExtractiveSummarizeAction>() { new ExtractiveSummarizeAction() }
            };

            // Start analysis process.
            AnalyzeActionsOperation operation = await client.StartAnalyzeActionsAsync(batchInput, actions);
            await operation.WaitForCompletionAsync();
            // View operation status.
            Console.WriteLine($"AnalyzeActions operation has completed");
            Console.WriteLine();

            Console.WriteLine($"Created On   : {operation.CreatedOn}");
            Console.WriteLine($"Expires On   : {operation.ExpiresOn}");
            Console.WriteLine($"Id           : {operation.Id}");
            Console.WriteLine($"Status       : {operation.Status}");

            Console.WriteLine();
            // View operation results.
            await foreach (AnalyzeActionsResult documentsInPage in operation.Value)
            {
                IReadOnlyCollection<ExtractiveSummarizeActionResult> summaryResults = documentsInPage.ExtractiveSummarizeResults;

                foreach (ExtractiveSummarizeActionResult summaryActionResults in summaryResults)
                {
                    if (summaryActionResults.HasError)
                    {
                        Console.WriteLine($"  Error!");
                        Console.WriteLine($"  Action error code: {summaryActionResults.Error.ErrorCode}.");
                        Console.WriteLine($"  Message: {summaryActionResults.Error.Message}");
                        continue;
                    }

                    foreach (ExtractiveSummarizeResult documentResults in summaryActionResults.DocumentsResults)
                    {
                        if (documentResults.HasError)
                        {
                            Console.WriteLine($"  Error!");
                            Console.WriteLine($"  Document error code: {documentResults.Error.ErrorCode}.");
                            Console.WriteLine($"  Message: {documentResults.Error.Message}");
                            continue;
                        }

                        Console.WriteLine($"  Extracted the following {documentResults.Sentences.Count} sentence(s):");
                        Console.WriteLine();

                        foreach (ExtractiveSummarySentence sentence in documentResults.Sentences)
                        {
                            Console.WriteLine($"  Sentence: {sentence.Text}");
                            Console.WriteLine();
                        }
                    }
                }
            }
        }


        // Example method for extracting information from healthcare-related text 
        static async Task healthExample(TextAnalyticsClient client)
        {
            string document = "Me recetaron ibuprofeno de 100 mg, que tomo dos veces al día.";

            List<string> batchInput = new List<string>()
            {
                document
            };
            AnalyzeHealthcareEntitiesOperation healthOperation = await client.StartAnalyzeHealthcareEntitiesAsync(batchInput);
            await healthOperation.WaitForCompletionAsync();

            await foreach (AnalyzeHealthcareEntitiesResultCollection documentsInPage in healthOperation.Value)
            {
                Console.WriteLine($"Results of Azure Text Analytics for health async model, version: \"{documentsInPage.ModelVersion}\"");
                Console.WriteLine("");

                foreach (AnalyzeHealthcareEntitiesResult entitiesInDoc in documentsInPage)
                {
                    if (!entitiesInDoc.HasError)
                    {
                        foreach (var entity in entitiesInDoc.Entities)
                        {
                            // view recognized healthcare entities
                            Console.WriteLine($"  Entity: {entity.Text}");
                            Console.WriteLine($"  Category: {entity.Category}");
                            Console.WriteLine($"  Offset: {entity.Offset}");
                            Console.WriteLine($"  Length: {entity.Length}");
                            Console.WriteLine($"  NormalizedText: {entity.NormalizedText}");
                        }
                        Console.WriteLine($"  Found {entitiesInDoc.EntityRelations.Count} relations in the current document:");
                        Console.WriteLine("");

                        // view recognized healthcare relations
                        foreach (HealthcareEntityRelation relations in entitiesInDoc.EntityRelations)
                        {
                            Console.WriteLine($"    Relation: {relations.RelationType}");
                            Console.WriteLine($"    For this relation there are {relations.Roles.Count} roles");

                            // view relation roles
                            foreach (HealthcareEntityRelationRole role in relations.Roles)
                            {
                                Console.WriteLine($"      Role Name: {role.Name}");

                                Console.WriteLine($"      Associated Entity Text: {role.Entity.Text}");
                                Console.WriteLine($"      Associated Entity Category: {role.Entity.Category}");
                                Console.WriteLine("");
                            }
                            Console.WriteLine("");
                        }
                    }
                    else
                    {
                        Console.WriteLine("  Error!");
                        Console.WriteLine($"  Document error code: {entitiesInDoc.Error.ErrorCode}.");
                        Console.WriteLine($"  Message: {entitiesInDoc.Error.Message}");
                    }
                    Console.WriteLine("");
                }
            }
        }

        //Generación de una respuesta desde un proyecto
        static void QuestionAnsweringExample(QuestionAnsweringClient client)
        {
            // Sin proyecto
            IEnumerable<TextDocument> records = new[]
            {
                new TextDocument("doc1", "Alimentación y carga.Se necesitan de dos a cuatro horas para cargar completamente la batería de Surface Pro 4 desde un estado vacío. " +
                         "Puede tomar más tiempo si estás usando tu Surface para actividades que consumen mucha energía, como juegos o transmisión de video, mientras lo estás cargando."),
                new TextDocument("doc2", "Puedes usar el puerto USB de la fuente de alimentación de tu Surface Pro 4 para cargar otros dispositivos, como un teléfono, mientras tu Surface se carga. " +
                         "El puerto USB de la fuente de alimentación solo sirve para cargar, no para transferir datos. Si quieres usar un dispositivo USB, conéctalo al puerto USB de tu Surface."),
            };

            AnswersFromTextOptions options = new AnswersFromTextOptions("Cuánto tiempo se tarda en cargar una surface?", records);
            Response<AnswersFromTextResult> response = client.GetAnswersFromText(options);

            foreach (TextAnswer answer in response.Value.Answers)
            {
                if (answer.Confidence > .8)
                {
                    string BestAnswer = response.Value.Answers[0].Answer;

                    Console.WriteLine($"Q:{options.Question}");
                    Console.WriteLine($"A:{BestAnswer}");
                    Console.WriteLine($"Confidence Score: ({response.Value.Answers[0].Confidence:P2})"); //:P2 converts the result to a percentage with 2 decimals of accuracy. 
                    break;
                }
                else
                {
                    Console.WriteLine($"Q:{options.Question}");
                    Console.WriteLine("Ninguna respuesta cumplió con el nivel de confianza solicitado.");
                    break;
                }
            }

            ///con proyecto
            //string projectName = "demo";
            //string deploymentName = "production";
            //string question = "How long should my Surface battery last?";

            
            //QuestionAnsweringProject project = new QuestionAnsweringProject(projectName, deploymentName);

            //Response<AnswersResult> response = client.GetAnswers(question, project);

            //foreach (KnowledgeBaseAnswer answer in response.Value.Answers)
            //{
            //    Console.WriteLine($"Q:{question}");
            //    Console.WriteLine($"A:{answer.Answer}");
            //}
        }

        // Example method for detecting opinions text. 
        static void SentimentAnalysisWithOpinionMiningExample(TextAnalyticsClient client)
        {
            var documents = new List<string>
            {
                //"La comida y el servicio eran inaceptables. Sin embargo, el conserje era agradable."
                "Entiende Jhony Romero, no te amo. Olvidate de mí, no quiero que me sigas trayendo obsequios."
            };

            AnalyzeSentimentResultCollection reviews = client.AnalyzeSentimentBatch(documents, options: new AnalyzeSentimentOptions()
            {
                IncludeOpinionMining = true
            });

            foreach (AnalyzeSentimentResult review in reviews)
            {
                Console.WriteLine($"Sentimiento del documento: {review.DocumentSentiment.Sentiment}\n");
                Console.WriteLine($"\tPuntuación positiva: {review.DocumentSentiment.ConfidenceScores.Positive:P2}");
                Console.WriteLine($"\tPuntuación negativa: {review.DocumentSentiment.ConfidenceScores.Negative:P2}");
                Console.WriteLine($"\tPuntuación neutral: {review.DocumentSentiment.ConfidenceScores.Neutral:0.00}\n");
                foreach (SentenceSentiment sentence in review.DocumentSentiment.Sentences)
                {
                    Console.WriteLine($"\tText: \"{sentence.Text}\"");
                    Console.WriteLine($"\tSentimiento de la oración: {sentence.Sentiment}");
                    Console.WriteLine($"\tSentence positive score: {sentence.ConfidenceScores.Positive:0.00}");
                    Console.WriteLine($"\tSentence negative score: {sentence.ConfidenceScores.Negative:0.00}");
                    Console.WriteLine($"\tSentence neutral score: {sentence.ConfidenceScores.Neutral:0.00}\n");

                    foreach (SentenceOpinion sentenceOpinion in sentence.Opinions)
                    {
                        Console.WriteLine($"\tTarget: {sentenceOpinion.Target.Text}, Value: {sentenceOpinion.Target.Sentiment}");
                        Console.WriteLine($"\tTarget positive score: {sentenceOpinion.Target.ConfidenceScores.Positive:0.00}");
                        Console.WriteLine($"\tTarget negative score: {sentenceOpinion.Target.ConfidenceScores.Negative:0.00}");
                        foreach (AssessmentSentiment assessment in sentenceOpinion.Assessments)
                        {
                            Console.WriteLine($"\t\tRelated Assessment: {assessment.Text}, Value: {assessment.Sentiment}");
                            Console.WriteLine($"\t\tRelated Assessment positive score: {assessment.ConfidenceScores.Positive:0.00}");
                            Console.WriteLine($"\t\tRelated Assessment negative score: {assessment.ConfidenceScores.Negative:0.00}");
                        }
                    }
                }
                Console.WriteLine($"\n");
            }
        }

        // Example method for recognizing entities and providing a link to an online data source.
        static void EntityLinkingExample(TextAnalyticsClient client)
        {
            var response = client.RecognizeLinkedEntities(
                "Microsoft was founded by Bill Gates and Paul Allen on April 4, 1975, " +
                "to develop and sell BASIC interpreters for the Altair 8800. " +
                "During his career at Microsoft, Gates held the positions of chairman, " +
                "chief executive officer, president and chief software architect, " +
                "while also being the largest individual shareholder until May 2014.");
            Console.WriteLine("Linked Entities:");
            foreach (var entity in response.Value)
            {
                Console.WriteLine($"\tName: {entity.Name},\tID: {entity.DataSourceEntityId},\tURL: {entity.Url}\tData Source: {entity.DataSource}");
                Console.WriteLine("\tMatches:");
                foreach (var match in entity.Matches)
                {
                    Console.WriteLine($"\t\tText: {match.Text}");
                    Console.WriteLine($"\t\tScore: {match.ConfidenceScore:F2}\n");
                }
            }
        }

        // Example method for extracting key phrases from text
        static void KeyPhraseExtractionExample(TextAnalyticsClient client)
        {
            var response = client.ExtractKeyPhrases(@"El Ing. Enrique tiene una empresa de tecnologia muy moderno y cuenta con un excelente personal, asi tambien es casado y tiene dos hijos.");

            // Printing key phrases
            Console.WriteLine("Key phrases:");

            foreach (string keyphrase in response.Value)
            {
                Console.WriteLine($"\t{keyphrase}");
            }
        }

        // Example method for detecting the language of text
        static void LanguageDetectionExample(TextAnalyticsClient client)
        {
            DetectedLanguage detectedLanguage = client.DetectLanguage("Hola, Enrique espero este example pueda servirte mucho.");
            Console.WriteLine("Language:");
            Console.WriteLine($"\t{detectedLanguage.Name},\tISO-6391: {detectedLanguage.Iso6391Name}\n");
        }

        // Example method for detecting sensitive information (PII) from text 
        static void RecognizePIIExample(TextAnalyticsClient client)
        {
            string document = "Llame a nuestra oficina al +51 986 687 645 o envíe un correo electrónico " +
                "a enrique.incio@evsoftconsultores.com, o puedo depositar en nuestra cuenta 4557-4859-8956-8596 " +
                " CI 4859685628566";

            PiiEntityCollection entities = client.RecognizePiiEntities(document).Value;

            Console.WriteLine($"Redacted Text: {entities.RedactedText}");
            if (entities.Count > 0)
            {
                Console.WriteLine($"Recognized {entities.Count} PII entit{(entities.Count > 1 ? "ies" : "y")}:");
                foreach (PiiEntity entity in entities)
                {
                    Console.WriteLine($"Text: {entity.Text}, Category: {entity.Category}, SubCategory: {entity.SubCategory}, Confidence score: {entity.ConfidenceScore}");
                }
            }
            else
            {
                Console.WriteLine("No entities were found.");
            }
        }

        static async Task Main(string[] args)
        {
            var client = new TextAnalyticsClient(endpoint, credentials);
            //RecognizePIIExample(client);
            //LanguageDetectionExample(client);
            //KeyPhraseExtractionExample(client);
            //EntityLinkingExample(client);
            //SentimentAnalysisWithOpinionMiningExample(client);

            //QuestionAnsweringClient client2 = new QuestionAnsweringClient(endpoint, credentials);
            //QuestionAnsweringExample(client2);

            //await healthExample(client);

            //await TextSummarizationExample(client);

            ConversationAnalysisClient client3 = new ConversationAnalysisClient(endpoint, credentials);
            await Summarization(client3);

            Console.Write("Press any key to exit.");
            Console.ReadKey();
        }

    }
}