import { createContext, useState } from "react";

export class UsrCont {
	isLogged: boolean = false;
	authToken: string | null = null;
}
export class UserContextType {
	user: UsrCont = new UsrCont();
	setUser: (arg: UsrCont) => void = () => {
		return;
	};
}

export const userContext = createContext(new UserContextType());
