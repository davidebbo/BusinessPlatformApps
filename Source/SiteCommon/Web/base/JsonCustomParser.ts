export class JsonCustomParser {

    public static isVariable(value: string): boolean {
        value = value.toString();
        if (value.startsWith('$(') && value.endsWith(')')) {
            return true;
        } else {
            return false;
        }
    }

    public static extractVariable(value: string): string {
        let intermediate: string = value.replace('$(', '');
        let result = intermediate.slice(0, intermediate.length - 1);
        let resultSplit = result.split(',');
        return resultSplit[0].trim();
    }

    public static isPermenantEntryIntoDataStore(value: string): boolean {
        let intermediate: string = value.replace('$(', '');
        let result = intermediate.slice(0, intermediate.length - 1);
        let resultSplit = result.split(',');

        for (let index =0; index< resultSplit.length; index++) {
            if (index < 1) {
                continue;
            }

            let param: string = resultSplit[index].trim().toLowerCase();
            let paramSplit = param.split('=');

            if (paramSplit[0] === 'issaved' && paramSplit[1] === 'true') {
                return true;
            }
        }

        return false;
    }
}