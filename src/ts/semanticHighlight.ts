import * as vscode from "vscode";
import { getCurrentFile } from "./source";
import {
	getClass as getClasses,
	getFunctions,
	getVariables,
} from "./helpers/documentAnalyze";

const tokenTypes = [
	"class",
	"variable",
	"function",
	"enum",
	"enumMember",
	"method",
	"undefinedVariable",
];
const tokenModifiers = ["declaration"];
export const semanticLegend = new vscode.SemanticTokensLegend(
	tokenTypes,
	tokenModifiers
);

export function getVariablesClient(text: string): string[] {
	return getVariables(text, getCurrentFile()!);
}

export function getFunctionsClient(text: string): string[] {
	return getFunctions(text, getCurrentFile()!);
}

export function getClassClient(text: string): string[] {
	return getClasses(text, getCurrentFile()!);
}