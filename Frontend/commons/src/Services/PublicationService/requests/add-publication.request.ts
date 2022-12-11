import { http } from "../../../http";
import {
  PublicationRequestType,
  PublicationResponseType,
  PublicationFromServerType,
  mapPublicationFromServer,
} from "../publicationTypes";

export const addPublication = async (
  publication: PublicationRequestType
): Promise<PublicationResponseType | undefined> => {
  const result = await http<PublicationFromServerType, PublicationRequestType>({
    path: "/publication",
    method: "post",
    body: publication,
  });

  if (result.ok && result.body) {
    return mapPublicationFromServer(result.body);
  } else {
    return undefined;
  }
};
