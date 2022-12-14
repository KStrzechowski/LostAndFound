export declare enum SinglePublicationVote {
    NoVote = "NoVote",
    Up = "Up",
    Down = "Down"
}
export declare enum PublicationType {
    LostSubject = "LostSubject",
    FoundSubject = "FoundSubject"
}
export declare enum PublicationState {
    Open = "Open",
    Closed = "Closed"
}
export type CategoryType = {
    id?: string;
    displayName?: string;
};
export type UserType = {
    id: string;
    username?: string;
};
export type PublicationRatingRequestType = {
    newPublicationVote: SinglePublicationVote;
};
export type PublicationStateRequestType = {
    publicationState: PublicationState;
};
export type PublicationRequestType = {
    title?: string;
    description?: string;
    incidentAddress?: string;
    incidentDate: Date;
    subjectCategoryId?: string;
    publicationType: PublicationType;
    publicationState?: PublicationState;
};
export type PublicationResponseType = {
    publicationId: string;
    title?: string;
    description?: string;
    subjectPhotoUrl?: string;
    incidentAddress?: string;
    incidentDate: Date;
    aggregateRaing: number;
    userVote: SinglePublicationVote;
    subjectCategoryId?: string;
    publicationType: PublicationType;
    publicationState: PublicationState;
    lastModificationDate: Date;
    creationDate: Date;
    author: UserType;
};
export type PublicationFromServerType = {
    publicationId: string;
    title?: string;
    description?: string;
    subjectPhotoUrl?: string;
    incidentAddress?: string;
    incidentDate: string;
    aggregateRaing: number;
    userVote: SinglePublicationVote;
    subjectCategoryId?: string;
    publicationType: PublicationType;
    publicationState: PublicationState;
    lastModificationDate: string;
    creationDate: string;
    author: UserType;
};
export declare const mapPublicationFromServer: (publication: PublicationFromServerType) => PublicationResponseType;
